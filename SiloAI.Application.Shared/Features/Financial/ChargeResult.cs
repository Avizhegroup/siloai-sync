namespace SiloAI.Application.Shared.Features;

/// <summary>Token consumption of a single AI call, as reported by the model provider.</summary>
public record TokenUsageInput(long InputTokens, long CachedTokens, long OutputTokens);

/// <summary>
/// Full result of the pricing formula with every input snapshotted, ready to be
/// persisted on a usage record so historical charges stay auditable after rate changes.
/// </summary>
public record ChargeResult(
    decimal CostUsd,
    decimal FxRateUsed,
    decimal CostToman,
    decimal MultiplierUsed,
    decimal FloorTomanUsed,
    decimal ChargeToman);

public enum ChargeOutcome
{
    Success,
    InsufficientBalance,
    DuplicateIgnored
}
