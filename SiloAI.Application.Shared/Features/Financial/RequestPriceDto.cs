namespace SiloAI.Application.Shared.Features;

/// <summary>
/// What one API request cost the customer, in both currencies. Returned as <c>price</c> in the
/// response of every billed request (null when the call was not billed, e.g. an admin call that
/// has no customer).
/// </summary>
public class RequestPriceDto
{
    /// <summary>The amount debited from the customer's credit, in Toman.</summary>
    public decimal Toman { get; set; }

    /// <summary>The same amount in US dollars (Toman ÷ exchange rate).</summary>
    public decimal Usd { get; set; }

    /// <summary>The global exchange rate (Toman per 1 USD) used for this request.</summary>
    public decimal ExchangeRate { get; set; }

    /// <summary>A request that cost nothing (for customers: calls that never reach a model).</summary>
    public static RequestPriceDto Free => new();

    public static RequestPriceDto From(ChargeResult charge)
        => Create(charge.ChargeToman, charge.FxRateUsed);

    /// <summary>The price of a request made of several billed steps (e.g. voice chat = speech-to-text + chat + text-to-speech).</summary>
    public static RequestPriceDto? Sum(IEnumerable<RequestPriceDto?> parts)
    {
        var present = parts.Where(p => p is not null).Select(p => p!).ToList();

        if (present.Count == 0)
            return null;

        var toman = present.Sum(p => p.Toman);
        var usd = present.Sum(p => p.Usd);

        return new RequestPriceDto
        {
            Toman = toman,
            Usd = usd,
            ExchangeRate = usd > 0 ? Math.Round(toman / usd, 2) : present[^1].ExchangeRate
        };
    }

    // Toman is rounded like the ledger stores it (2 decimals); dollars keep 6 so tiny requests aren't shown as 0.
    private static RequestPriceDto Create(decimal chargeToman, decimal exchangeRate) => new()
    {
        Toman = Math.Round(chargeToman, 2, MidpointRounding.AwayFromZero),
        Usd = exchangeRate > 0 ? Math.Round(chargeToman / exchangeRate, 6, MidpointRounding.AwayFromZero) : 0m,
        ExchangeRate = exchangeRate
    };
}
