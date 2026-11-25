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


/// <summary>How one customer is billed (their own columns on the customer record — nothing global).</summary>
public record CustomerPricingProfile(decimal TomanPerUsd, decimal Multiplier, decimal FloorToman, decimal FloorUsd)
{
    /// <summary>Returns a human-readable reason when the profile can't be used to bill, else null.</summary>
    public static string? Validate(decimal tomanPerUsd, decimal multiplier, decimal floorToman, decimal floorUsd)
    {
        if (tomanPerUsd <= 0) return "نرخ تبدیل دلار به تومان باید بزرگ‌تر از صفر باشد.";
        if (multiplier <= 0) return "ضریب باید بزرگ‌تر از صفر باشد.";
        if (floorToman < 0 || floorUsd < 0) return "حداقل مبلغ نمی‌تواند منفی باشد.";
        return null;
    }
}

/// <summary>
/// The pricing formula, in one place and free of I/O:
/// costToman = costUsd × rate; charge = max(costToman × M, floorToman, rate × floorUsd).
/// </summary>
public static class PricingFormula
{
    public static ChargeResult Compute(decimal costUsd, CustomerPricingProfile pricing)
    {
        var costToman = costUsd * pricing.TomanPerUsd;

        var chargeToman = Math.Max(
            costToman * pricing.Multiplier,
            Math.Max(pricing.FloorToman, pricing.TomanPerUsd * pricing.FloorUsd));

        return new ChargeResult(
            costUsd, pricing.TomanPerUsd, costToman, pricing.Multiplier, pricing.FloorToman, chargeToman);
    }
}
