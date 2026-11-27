using Microsoft.AspNetCore.Mvc;
using SiloAI.Application.Api;
using SiloAI.Application.Shared.Features;

namespace SiloAI.Api.Infrastructure;

/// <summary>
/// Writes a voice-chat answer: the audio as the body, and the conversation id / transcript / reply
/// text (URL-escaped — headers can't carry raw Persian text) as response headers.
/// </summary>
public sealed class VoiceChatActionResult(RagVoiceChatResult result) : IActionResult
{
    public Task ExecuteResultAsync(ActionContext context)
    {
        var headers = context.HttpContext.Response.Headers;

        headers["X-Conversation-Id"] = result.ConversationId.ToString();
        headers["X-Transcript"] = Uri.EscapeDataString(result.Transcript);
        headers["X-Response-Text"] = Uri.EscapeDataString(result.ResponseText);
        AddPriceHeaders(headers, result.Price);
        headers.AccessControlExposeHeaders = "X-Conversation-Id,X-Transcript,X-Response-Text,X-Price-Toman,X-Price-Usd,X-Exchange-Rate";

        return new FileContentResult(result.Audio, result.AudioContentType).ExecuteResultAsync(context);
    }

    /// <summary>The body is audio, so the price travels as headers (only when the call was billed).</summary>
    public static void AddPriceHeaders(IHeaderDictionary headers, RequestPriceDto? price)
    {
        if (price is null)
            return;

        headers["X-Price-Toman"] = price.Toman.ToString(System.Globalization.CultureInfo.InvariantCulture);
        headers["X-Price-Usd"] = price.Usd.ToString(System.Globalization.CultureInfo.InvariantCulture);
        headers["X-Exchange-Rate"] = price.ExchangeRate.ToString(System.Globalization.CultureInfo.InvariantCulture);
    }
}
