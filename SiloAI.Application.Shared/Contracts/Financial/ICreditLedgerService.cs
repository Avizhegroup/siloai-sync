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

    /// <summary>
    /// Balance plus the customer's credit policy. The default (kept so simple implementations and
    /// test fakes still work) treats every customer as "stop on insufficient credit".
    /// </summary>
    async Task<CreditStatus> GetCreditStatusAsync(int customerId, CancellationToken cancellationToken)
        => new CreditStatus(await GetBalanceAsync(customerId, cancellationToken), StopOnInsufficientCredit: true);
}

/// <summary>
/// A customer's spendable position. <see cref="StopOnInsufficientCredit"/> false means usage is
/// never refused for lack of credit and the balance may go negative.
/// </summary>
public record CreditStatus(decimal BalanceToman, bool StopOnInsufficientCredit)
{
    /// <summary>True when the customer must be refused before any (paid) provider call is made.</summary>
    public bool MustBlock => StopOnInsufficientCredit && BalanceToman <= 0;
}
