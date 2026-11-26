using SiloAI.Application.Shared.Contracts.Financial;

namespace SiloAI.Application.Api.Services;

/// <summary>
/// Implements the pricing formula: costUsd from the resolved AiModel's price list,
/// costToman = costUsd × the GLOBAL exchange rate (latest row of tbl_FxRateSettings with
/// EffectiveFrom &lt;= now), charge = costToman × the paying customer's multiplier.
/// Missing configuration fails fast instead of silently falling back to a default.
/// </summary>
public class PricingEngine(AiApiContext dbContext) : IPricingEngine
{
    public async Task<ChargeResult> CalculateAsync(
        int customerId, TokenUsageInput usage, UsageFeature feature, Guid aiModelId, CancellationToken cancellationToken)
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
                "No active FX rate found. Add a rate on the Financial page (tbl_FxRateSettings needs a row whose EffectiveFrom <= now).");

        var multiplier = await dbContext.Customers
            .AsNoTracking()
            .Where(c => c.Id == customerId)
            .Select(c => (decimal?)c.Multiplier)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException($"Customer '{customerId}' was not found, so it cannot be priced.");

        var problem = PricingFormula.ValidateMultiplier(multiplier);
        if (problem is not null)
            throw new InvalidOperationException($"Customer '{customerId}' has an invalid multiplier: {problem}");

        // Per-token / per-unit prices come from the resolved AiModel record (admin-entered on the AI
        // Models page): a model's price list travels with the model itself.
        var modelPricing = await dbContext.AiModels
            .AsNoTracking()
            .Where(x => x.Id == aiModelId)
            .Select(x => new ModelPriceList(
                x.InputPricePerMillionTokens, x.OutputPricePerMillionTokens, x.CachedInputPricePerMillionTokens,
                x.PricePerImageUsd, x.PricePerAudioMinuteUsd, x.PricePerMillionCharactersUsd))
            .FirstOrDefaultAsync(cancellationToken);

        if (modelPricing is null)
            throw new InvalidOperationException(
                $"AiModel '{aiModelId}' was not found. It may have been deleted after being resolved for this call.");

        return PricingFormula.Compute(UsageCostCalculator.ComputeCostUsd(usage, modelPricing), fxRate, multiplier);
    }
}
