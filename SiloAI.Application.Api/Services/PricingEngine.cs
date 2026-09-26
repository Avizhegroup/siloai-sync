using Microsoft.Extensions.Configuration;
using SiloAI.Application.Shared.Contracts.Financial;

namespace SiloAI.Application.Api.Services;

/// <summary>
/// Implements the pricing formula:
/// costUsd from the model price list, costToman = costUsd × fxRate,
/// chargeToman = max(costToman × M, floorToman, fxRate × floorUsd).
/// Rates and multipliers come from the latest row with EffectiveFrom &lt;= now.
/// Missing configuration fails fast instead of silently falling back to a default.
/// </summary>
public class PricingEngine(AiApiContext dbContext, IConfiguration configuration) : IPricingEngine
{
    public async Task<ChargeResult> CalculateAsync(TokenUsageInput usage, UsageFeature feature, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

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

        var modelName = configuration["OpenAI:MainModel"];

        var inputPrice = configuration.GetValue<decimal>($"AiPricing:Models:{modelName}:InputPerMillionTokens");
        var outputPrice = configuration.GetValue<decimal>($"AiPricing:Models:{modelName}:OutputPerMillionTokens");
        var cachedInputPrice = configuration.GetValue<decimal>($"AiPricing:Models:{modelName}:CachedInputPerMillionTokens");

        var normalInputTokens = Math.Max(0, usage.InputTokens - usage.CachedTokens);

        var costUsd =
            (normalInputTokens / 1_000_000m * inputPrice)
            + (usage.CachedTokens / 1_000_000m * cachedInputPrice)
            + (usage.OutputTokens / 1_000_000m * outputPrice);

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
