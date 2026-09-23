using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using SiloAI.Application.Shared.Contracts.Financial;

namespace SiloAI.Application.Api.Services;

/// <summary>
/// The only place balances change. Debits use a single atomic conditional UPDATE
/// (WHERE balance &gt;= amount) so two concurrent requests can never both succeed when
/// funds cover only one of them — the database row lock guarantees it. The ledger row
/// and the usage record are inserted in the same database transaction, and the unique
/// index on IdempotencyKey is the real guard against double-charging on retries.
///
/// Uses its own <see cref="AiApiContext"/> instance (via the scoped factory) so the
/// ledger transaction never sweeps up unrelated entities tracked by a caller's context.
/// </summary>
public class CreditLedgerService(IServiceScopeFactory scopeFactory) : ICreditLedgerService
{
    public async Task<ChargeOutcome> ChargeAsync(
        int customerId, ChargeResult charge, UsageRecord usageRecord,
        string idempotencyKey, CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AiApiContext>();

        // Fast path: drop retried requests before opening a transaction. The unique
        // index is still the real guarantee against a race between two live requests.
        var alreadyCharged = await dbContext.LedgerTransactions
            .AsNoTracking()
            .AnyAsync(t => t.IdempotencyKey == idempotencyKey, cancellationToken);

        if (alreadyCharged)
            return ChargeOutcome.DuplicateIgnored;

        await using var dbTransaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var now = DateTime.UtcNow;

        // Atomic conditional debit — no read-then-write.
        var updatedRows = await dbContext.LedgerAccounts
            .Where(a => a.CustomerId == customerId && a.BalanceToman >= charge.ChargeToman)
            .ExecuteUpdateAsync(s => s
                .SetProperty(a => a.BalanceToman, a => a.BalanceToman - charge.ChargeToman)
                .SetProperty(a => a.UpdatedAt, now), cancellationToken);

        if (updatedRows == 0)
        {
            await dbTransaction.RollbackAsync(cancellationToken);
            return ChargeOutcome.InsufficientBalance;
        }

        var account = await dbContext.LedgerAccounts
            .AsNoTracking()
            .Where(a => a.CustomerId == customerId)
            .Select(a => new { a.Id, a.BalanceToman })
            .SingleAsync(cancellationToken);

        dbContext.UsageRecords.Add(usageRecord);

        dbContext.LedgerTransactions.Add(new LedgerTransaction
        {
            Id = Guid.NewGuid(),
            AccountId = account.Id,
            Type = LedgerTransactionType.Usage,
            Amount = -charge.ChargeToman,
            BalanceAfter = account.BalanceToman,
            IdempotencyKey = idempotencyKey,
            UsageRecordId = usageRecord.Id,
            Description = usageRecord.Feature.ToDisplay(),
            CreatedAt = now
        });

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            await dbTransaction.CommitAsync(cancellationToken);
            return ChargeOutcome.Success;
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            // Race: another concurrent request committed the same idempotency key first.
            await dbTransaction.RollbackAsync(cancellationToken);
            return ChargeOutcome.DuplicateIgnored;
        }
    }

    public async Task<ChargeOutcome> TopUpAsync(
        int customerId, decimal amountToman, string reference, string? description, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(reference))
            throw new InvalidOperationException("A top-up reference (e.g. the payment gateway transaction id) is required.");

        await using var scope = scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AiApiContext>();

        var alreadyToppedUp = await dbContext.LedgerTransactions
            .AsNoTracking()
            .AnyAsync(t => t.IdempotencyKey == reference, cancellationToken);

        if (alreadyToppedUp)
            return ChargeOutcome.DuplicateIgnored;

        await using var dbTransaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var now = DateTime.UtcNow;

        var updatedRows = await dbContext.LedgerAccounts
            .Where(a => a.CustomerId == customerId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(a => a.BalanceToman, a => a.BalanceToman + amountToman)
                .SetProperty(a => a.UpdatedAt, now), cancellationToken);

        if (updatedRows == 0)
        {
            await dbTransaction.RollbackAsync(cancellationToken);
            throw new InvalidOperationException(
                $"Customer '{customerId}' has no ledger account. Accounts are created together with the customer.");
        }

        var account = await dbContext.LedgerAccounts
            .AsNoTracking()
            .Where(a => a.CustomerId == customerId)
            .Select(a => new { a.Id, a.BalanceToman })
            .SingleAsync(cancellationToken);

        dbContext.LedgerTransactions.Add(new LedgerTransaction
        {
            Id = Guid.NewGuid(),
            AccountId = account.Id,
            Type = LedgerTransactionType.TopUp,
            Amount = amountToman,
            BalanceAfter = account.BalanceToman,
            IdempotencyKey = reference,
            Description = description,
            CreatedAt = now
        });

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            await dbTransaction.CommitAsync(cancellationToken);
            return ChargeOutcome.Success;
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            await dbTransaction.RollbackAsync(cancellationToken);
            return ChargeOutcome.DuplicateIgnored;
        }
    }

    public async Task<decimal> GetBalanceAsync(int customerId, CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AiApiContext>();

        return await dbContext.LedgerAccounts
            .AsNoTracking()
            .Where(a => a.CustomerId == customerId)
            .Select(a => a.BalanceToman)
            .SingleOrDefaultAsync(cancellationToken);
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException exception)
    {
        for (Exception? ex = exception; ex is not null; ex = ex.InnerException)
        {
            // SQL Server: 2601 (unique index) / 2627 (primary key / unique constraint)
            if (ex is SqlException { Number: 2601 or 2627 })
                return true;

            // Provider-agnostic fallback for other engines (e.g. SQLite error 19).
            if (ex.GetType().Name == "SqliteException" && ex.Message.Contains("UNIQUE constraint failed"))
                return true;
        }

        return false;
    }
}

