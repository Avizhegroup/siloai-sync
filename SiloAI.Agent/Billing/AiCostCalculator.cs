using Microsoft.EntityFrameworkCore;
using SiloAI.Application.Shared.Features;
using SiloAI.Domains;

namespace SiloAI.Agent;

/// <summary>
/// Legacy informational cost calculator (feeds the PriceUsage field returned in chat responses).
/// Prices are looked up from the resolved AiModel record — NOT from appsettings — so this
/// reflects whichever model was actually called for this customer/feature, not a fixed global one.
/// The real balance-affecting charge is computed separately by IPricingEngine/ICreditLedgerService;
/// this exists only to keep the response DTOs' PriceUsage field populated.
/// </summary>
public class AiCostCalculator(AiApiContext context)
{
    public async Task<decimal> CalculateAsync(ChatTokenUsageDto tokenUsage, string? modelIdentifier, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(modelIdentifier))
            return 0m;

        var pricing = await context.AiModels
            .AsNoTracking()
            .Where(m => m.Identifier == modelIdentifier)
            .Select(m => new { m.InputPricePerMillionTokens, m.OutputPricePerMillionTokens, m.CachedInputPricePerMillionTokens })
            .FirstOrDefaultAsync(cancellationToken);

        if (pricing is null)
            return 0m;

        var normalInputTokens = Math.Max(0, tokenUsage.InputTokenCount - tokenUsage.CachedInputTokenCount);

        var priceUsage =
            (normalInputTokens / 1_000_000m * pricing.InputPricePerMillionTokens)
            +
            (tokenUsage.CachedInputTokenCount / 1_000_000m * pricing.CachedInputPricePerMillionTokens)
            +
            (tokenUsage.OutputTokenCount / 1_000_000m * pricing.OutputPricePerMillionTokens);

        return priceUsage;
    }
}
