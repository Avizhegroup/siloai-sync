using SiloAI.Application.Shared.Features;
using Xunit;

namespace SiloAI.Tests;

public class UsageCostCalculatorTests
{
    private static readonly ModelPriceList Tokens = new(InputPerMillionTokens: 2m, OutputPerMillionTokens: 8m, CachedInputPerMillionTokens: 0.5m);

    [Fact]
    public void Splits_input_into_normal_and_cached_buckets_and_adds_output()
    {
        // 600k normal @2 + 400k cached @0.5 + 250k output @8 = 1.2 + 0.2 + 2.0
        var cost = UsageCostCalculator.ComputeCostUsd(new TokenUsageInput(1_000_000, 400_000, 250_000), Tokens);

        Assert.Equal(3.4m, cost);
    }

    [Fact]
    public void Cached_tokens_are_never_billed_beyond_the_input_count()
    {
        var cost = UsageCostCalculator.ComputeCostUsd(new TokenUsageInput(100_000, 900_000, 0), Tokens);

        Assert.Equal(100_000 / 1_000_000m * 0.5m, cost);
    }

    [Fact]
    public void Zero_usage_costs_nothing_and_negative_counters_are_ignored()
    {
        Assert.Equal(0m, UsageCostCalculator.ComputeCostUsd(new TokenUsageInput(0, 0, 0), Tokens));
        Assert.Equal(0m, UsageCostCalculator.ComputeCostUsd(new TokenUsageInput(-5, -5, -5, -1, -1m, -1), Tokens));
    }

    [Fact]
    public void Images_are_billed_per_image()
    {
        var prices = Tokens with { PerImage = 0.04m };

        var cost = UsageCostCalculator.ComputeCostUsd(new TokenUsageInput(0, 0, 0, ImageCount: 3), prices);

        Assert.Equal(0.12m, cost);
    }

    [Fact]
    public void Audio_is_billed_per_minute_from_seconds()
    {
        var prices = Tokens with { PerAudioMinute = 0.006m };

        var cost = UsageCostCalculator.ComputeCostUsd(new TokenUsageInput(0, 0, 0, AudioSeconds: 90m), prices);

        Assert.Equal(0.009m, cost);
    }

    [Fact]
    public void Synthesized_characters_are_billed_per_million()
    {
        var prices = Tokens with { PerMillionCharacters = 30m };

        var cost = UsageCostCalculator.ComputeCostUsd(new TokenUsageInput(0, 0, 0, Characters: 2_000), prices);

        Assert.Equal(0.06m, cost);
    }

    [Fact]
    public void Token_and_media_costs_add_up()
    {
        var prices = Tokens with { PerImage = 0.04m };

        var cost = UsageCostCalculator.ComputeCostUsd(new TokenUsageInput(1_000_000, 0, 0, ImageCount: 1), prices);

        Assert.Equal(2.04m, cost);
    }

    [Fact]
    public void Unpriced_buckets_contribute_zero()
    {
        var cost = UsageCostCalculator.ComputeCostUsd(
            new TokenUsageInput(10, 0, 10, ImageCount: 5, AudioSeconds: 60m, Characters: 100),
            new ModelPriceList(0, 0, 0));

        Assert.Equal(0m, cost);
    }
}
