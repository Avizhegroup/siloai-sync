using SiloAI.Application.Shared.Contracts.Financial;

namespace SiloAI.Application.Api.Services;

/// <summary>
/// Implements the pricing formula:
/// costUsd from the resolved AiModel's price list, costToman = costUsd × fxRate,
/// chargeToman = max(costToman × M, floorToman, fxRate × floorUsd).
/// Rates and multipliers come from the latest row with EffectiveFrom &lt;= now.
/// Missing configuration fails fast instead of silently falling back to a default.
/// </summary>
public class PricingEngine(AiApiContext dbContext) : IPricingEngine
{
    public async Task<ChargeResult> CalculateAsync(TokenUsageInput usage, UsageFeature feature, Guid aiModelId, CancellationToken cancellationToken)
    {
        var now = DateTime.Now;

        var fxRate = await dbContext.FxRateSettings
            .AsNoTracking()
            .Where(x => x.EffectiveFrom <= now)
            .OrderByDescending(x => x.EffectiveFrom)
            .Select(x => x.TomanPerUsd)
            .FirstOrDefaultAsync(cancellationToken);

        if (fxRate <= 0)
            throw new InvalidOperationException(
                "No active FX rate found. Seed tbl_FxRateSettings with a row whose EffectiveFrom <= now.");

        var pricing = await dbContext.PricingSettings
            .AsNoTracking()
            .Where(x => x.Feature == feature && x.EffectiveFrom <= now)
            .OrderByDescending(x => x.EffectiveFrom)
            .FirstOrDefaultAsync(cancellationToken);

        if (pricing is null)
            throw new InvalidOperationException(
                $"No active pricing setting found for feature '{feature}'. Seed tbl_PricingSettings with a row whose EffectiveFrom <= now.");

        // Per-token prices now come from the resolved AiModel record (admin-entered on the AI
        // Models page), not from appsettings — a model's price list travels with the model
        // itself, so different customers/features using different models are priced correctly.
        var modelPricing = await dbContext.AiModels
            .AsNoTracking()
            .Where(x => x.Id == aiModelId)
            .Select(x => new { x.InputPricePerMillionTokens, x.OutputPricePerMillionTokens, x.CachedInputPricePerMillionTokens })
            .FirstOrDefaultAsync(cancellationToken);

        if (modelPricing is null)
            throw new InvalidOperationException(
                $"AiModel '{aiModelId}' was not found. It may have been deleted after being resolved for this call.");

        var normalInputTokens = Math.Max(0, usage.InputTokens - usage.CachedTokens);

        var costUsd =
            (normalInputTokens / 1_000_000m * modelPricing.InputPricePerMillionTokens)
            + (usage.CachedTokens / 1_000_000m * modelPricing.CachedInputPricePerMillionTokens)
            + (usage.OutputTokens / 1_000_000m * modelPricing.OutputPricePerMillionTokens);

        var costToman = costUsd * fxRate;

        var chargeToman = Math.Max(
            costToman * pricing.Multiplier,
            Math.Max(pricing.FloorToman, fxRate * pricing.FloorUsd));

        return new ChargeResult(
            costUsd,
            fxRate,
            costToman,
            pricing.Multiplier,
            pricing.FloorToman,
            chargeToman);
    }
}
