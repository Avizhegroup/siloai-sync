namespace SiloAI.Application.Api.Services;

/// <summary>
/// Pre-flight balance check used before an AI call is made, so a customer with no credit never
/// causes provider cost we cannot recover. The ledger is the single source of truth.
/// </summary>
public static class CreditGuard
{
    /// <summary>Throws <see cref="InsufficientCreditException"/> when the customer is set to stop on insufficient credit and the balance is not above zero. Null customer = internal/unmetered call.</summary>
    public static async Task EnsureCreditAsync(this ICreditLedgerService ledger, int? customerId, CancellationToken cancellationToken)
    {
        if (customerId is null)
            return;

        // Only customers flagged "stop on insufficient credit" are refused; the others keep using the
        // service and their balance is allowed to go negative.
        var status = await ledger.GetCreditStatusAsync(customerId.Value, cancellationToken);
        if (status.MustBlock)
            throw new InsufficientCreditException();
    }

    /// <summary>Clamps a long counter into the int columns of <see cref="UsageRecord"/>.</summary>
    public static int ToInt(long value) => (int)Math.Clamp(value, 0, int.MaxValue);
}
