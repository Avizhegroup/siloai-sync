namespace SiloAI.Application.Shared.Features;

/// <summary>Token consumption of a single AI call, as reported by the model provider.</summary>
public record TokenUsageInput(
    long InputTokens,
    long CachedTokens,
    long OutputTokens,
    int ImageCount = 0,
    decimal AudioSeconds = 0m,
    long Characters = 0);

/// <summary>The price list of one model, as entered on the AI Models page (all USD).</summary>
public record ModelPriceList(
    decimal InputPerMillionTokens,
    decimal OutputPerMillionTokens,
    decimal CachedInputPerMillionTokens,
    decimal PerImage = 0m,
    decimal PerAudioMinute = 0m,
    decimal PerMillionCharacters = 0m);

/// <summary>
/// Pure cost arithmetic (no I/O) so every AI endpoint is priced by the same formula:
/// normal input (input − cached) + cached input + output tokens + per-image + per-audio-minute +
/// per-character. A bucket whose price is zero contributes zero.
/// </summary>
public static class UsageCostCalculator
{
    public static decimal ComputeCostUsd(TokenUsageInput usage, ModelPriceList prices)
    {
        var input = Math.Max(0, usage.InputTokens);
        // Providers report cached tokens as a subset of input tokens; never bill more cached than input.
        var cached = Math.Min(Math.Max(0, usage.CachedTokens), input);
        var normalInput = input - cached;
        var output = Math.Max(0, usage.OutputTokens);

        return normalInput / 1_000_000m * prices.InputPerMillionTokens
             + cached / 1_000_000m * prices.CachedInputPerMillionTokens
             + output / 1_000_000m * prices.OutputPerMillionTokens
             + Math.Max(0, usage.ImageCount) * prices.PerImage
             + Math.Max(0m, usage.AudioSeconds) / 60m * prices.PerAudioMinute
             + Math.Max(0, usage.Characters) / 1_000_000m * prices.PerMillionCharacters;
    }
}

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


/// <summary>
/// The pricing formula, in one place and free of I/O:
/// costToman = costUsd × global rate; charge = costToman × the customer's multiplier.
/// </summary>
public static class PricingFormula
{
    /// <summary>Returns a human-readable reason when the multiplier can't be used to bill, else null.</summary>
    public static string? ValidateMultiplier(decimal multiplier)
        => multiplier <= 0 ? "ضریب باید بزرگ‌تر از صفر باشد." : null;

    public static ChargeResult Compute(decimal costUsd, decimal tomanPerUsd, decimal multiplier)
    {
        var costToman = costUsd * tomanPerUsd;

        // FloorTomanUsed is kept on the result (and usage record) only for history; there is no minimum charge any more.
        return new ChargeResult(costUsd, tomanPerUsd, costToman, multiplier, 0m, costToman * multiplier);
    }
}
