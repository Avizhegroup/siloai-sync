using SiloAI.Application.Shared.Contracts.Financial;

namespace SiloAI.Application.Api.Services;

/// <summary>
/// Implements the pricing formula for one customer: costUsd from the resolved AiModel's price list,
/// then <see cref="PricingFormula"/> with the customer's own exchange rate, multiplier and floors.
/// There is no global pricing configuration; a customer with an unusable profile fails fast
/// instead of being billed with made-up numbers.
/// </summary>
public class PricingEngine(AiApiContext dbContext) : IPricingEngine
{
    public async Task<ChargeResult> CalculateAsync(
        int customerId, TokenUsageInput usage, UsageFeature feature, Guid aiModelId, CancellationToken cancellationToken)
    {
        var profile = await dbContext.Customers
            .AsNoTracking()
            .Where(c => c.Id == customerId)
            .Select(c => new CustomerPricingProfile(c.TomanPerUsd, c.Multiplier, c.FloorToman, c.FloorUsd))
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException($"Customer '{customerId}' was not found, so it cannot be priced.");

        var problem = CustomerPricingProfile.Validate(profile.TomanPerUsd, profile.Multiplier, profile.FloorToman, profile.FloorUsd);
        if (problem is not null)
            throw new InvalidOperationException($"Customer '{customerId}' has an invalid pricing profile: {problem}");

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

        return PricingFormula.Compute(UsageCostCalculator.ComputeCostUsd(usage, modelPricing), profile);
    }
}
