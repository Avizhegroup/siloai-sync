namespace SiloAI.Application.Shared.Contracts.Financial;

/// <summary>
/// Computes the charge for a single AI call from the active pricing settings.
/// Pure calculation only — never touches balances. Missing configuration is a
/// fail-fast error (no silent fallback numbers).
/// </summary>
public interface IPricingEngine
{
    Task<ChargeResult> CalculateAsync(TokenUsageInput usage, UsageFeature feature, CancellationToken cancellationToken);
}
