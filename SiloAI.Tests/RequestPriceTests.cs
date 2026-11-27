using SiloAI.Application.Shared.Features;
using Xunit;

namespace SiloAI.Tests;

public class RequestPriceTests
{
    private static ChargeResult Charge(decimal chargeToman, decimal rate) => new(0m, rate, 0m, 1m, 0m, chargeToman);

    [Fact]
    public void The_price_is_the_charge_in_toman_and_the_same_amount_in_dollars()
    {
        var p = RequestPriceDto.From(Charge(200000m, 100000m));

        Assert.Equal(200000m, p.Toman);
        Assert.Equal(2m, p.Usd);
        Assert.Equal(100000m, p.ExchangeRate);
    }

    [Fact]
    public void Toman_is_rounded_like_the_ledger_and_tiny_dollar_amounts_are_not_lost()
    {
        var p = RequestPriceDto.From(Charge(123.456m, 100000m));

        Assert.Equal(123.46m, p.Toman);
        Assert.Equal(0.001235m, p.Usd);
    }

    [Fact]
    public void A_zero_exchange_rate_does_not_divide_by_zero()
        => Assert.Equal(0m, RequestPriceDto.From(Charge(100m, 0m)).Usd);

    [Fact]
    public void Free_costs_nothing()
    {
        Assert.Equal(0m, RequestPriceDto.Free.Toman);
        Assert.Equal(0m, RequestPriceDto.Free.Usd);
    }

    [Fact]
    public void Sum_adds_up_the_steps_of_one_request()
    {
        var total = RequestPriceDto.Sum(
        [
            RequestPriceDto.From(Charge(1000m, 100000m)),
            null,
            RequestPriceDto.From(Charge(3000m, 100000m))
        ]);

        Assert.Equal(4000m, total!.Toman);
        Assert.Equal(0.04m, total.Usd);
        Assert.Equal(100000m, total.ExchangeRate);
    }

    [Fact]
    public void Sum_of_nothing_billed_is_null()
        => Assert.Null(RequestPriceDto.Sum([null, null]));

    [Fact]
    public void Every_documented_billed_response_carries_a_price()
    {
        foreach (var t in new[]
        {
            typeof(RagChatResponse), typeof(ImageGenerationResponse), typeof(OcrResponse),
            typeof(ImageAnalysisResponse), typeof(TranscriptionResponse)
        })
        {
            var prop = t.GetProperty("Price");
            Assert.NotNull(prop);
            Assert.Equal(typeof(RequestPriceDto), prop!.PropertyType);
        }
    }

    [Fact]
    public void The_price_serializes_as_camelCase_toman_usd_exchangeRate()
    {
        var json = System.Text.Json.JsonSerializer.Serialize(
            new ImageAnalysisResponse { Price = new RequestPriceDto { Toman = 5m, Usd = 0.1m, ExchangeRate = 50m } },
            new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web));

        Assert.Contains("\"price\":{\"toman\":5,\"usd\":0.1,\"exchangeRate\":50}", json);
    }
}
