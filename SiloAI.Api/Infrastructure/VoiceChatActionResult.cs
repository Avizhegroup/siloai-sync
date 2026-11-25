using Microsoft.AspNetCore.Mvc;
using SiloAI.Application.Api;

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
        headers.AccessControlExposeHeaders = "X-Conversation-Id,X-Transcript,X-Response-Text";

        return new FileContentResult(result.Audio, result.AudioContentType).ExecuteResultAsync(context);
    }
}
