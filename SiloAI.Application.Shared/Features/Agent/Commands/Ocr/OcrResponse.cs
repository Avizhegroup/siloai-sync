namespace SiloAI.Application.Shared.Features;

public class OcrResponse
{
    public string ExtractedText { get; set; }

    /// <summary>What this request cost, in Toman and USD. Null when the call was not billed.</summary>
    public RequestPriceDto? Price { get; set; }
}
