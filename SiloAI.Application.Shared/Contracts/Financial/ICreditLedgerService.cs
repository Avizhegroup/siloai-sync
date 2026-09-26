namespace SiloAI.Application.Shared.Contracts.Financial;

/// <summary>
/// The only entry point for changing a customer's balance. The balance is a cached
/// aggregate of the append-only ledger; nobody else may write to it directly.
/// </summary>
public interface ICreditLedgerService
{
    /// <summary>
    /// Atomically debits the customer's account and records the usage + ledger
    /// transaction in a single database transaction. The idempotency key (unique at
    /// the database level) guarantees a retried request never charges twice.
    /// </summary>
    Task<ChargeOutcome> ChargeAsync(
        int customerId, ChargeResult charge, UsageRecord usageRecord,
        string idempotencyKey, CancellationToken cancellationToken);

    /// <summary>
    /// Credits the customer's account. <paramref name="reference"/> must be the payment
    /// gateway transaction id so the same top-up is never recorded twice.
    /// </summary>
    Task<ChargeOutcome> TopUpAsync(
        int customerId, decimal amountToman, string reference, string? description, CancellationToken cancellationToken);

    Task<decimal> GetBalanceAsync(int customerId, CancellationToken cancellationToken);
}
