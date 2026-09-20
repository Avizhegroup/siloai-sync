using Microsoft.Extensions.Configuration;
using SiloAI.Application.Shared.Features;

namespace SiloAI.Agent;

public class AiCostCalculator(IConfiguration configuration)
{
    public decimal Calculate(ChatTokenUsageDto tokenUsage, decimal priceMultiplier = 1.0m)
    {
        var effectiveMultiplier = priceMultiplier < 1.0m ? 1.0m : priceMultiplier;

        var modelName = configuration["OpenAI:MainModel"];

        var inputPrice = configuration.GetValue<decimal>($"AiPricing:Models:{modelName}:InputPerMillionTokens");

        var outputPrice = configuration.GetValue<decimal>($"AiPricing:Models:{modelName}:OutputPerMillionTokens");

        var cachedInputPrice = configuration.GetValue<decimal>($"AiPricing:Models:{modelName}:CachedInputPerMillionTokens");

        var normalInputTokens = Math.Max(0, tokenUsage.InputTokenCount - tokenUsage.CachedInputTokenCount);

        var priceUsage =
            (normalInputTokens / 1_000_000m * inputPrice)
            +
            (tokenUsage.CachedInputTokenCount / 1_000_000m * cachedInputPrice)
            +
            (tokenUsage.OutputTokenCount / 1_000_000m * outputPrice);

        return priceUsage * effectiveMultiplier;
    }
}
