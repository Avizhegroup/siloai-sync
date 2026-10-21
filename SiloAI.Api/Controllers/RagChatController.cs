using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SiloAI.Api.Auth;
using SiloAI.Application.Api;
using SiloAI.Shared;
using System.Text;

namespace SiloAI.Api.Controllers;

[ApiController]
[Route("api/rag/chat")]
[Authorize(AuthenticationSchemes =
    $"{JwtBearerDefaults.AuthenticationScheme},{ApiKeyAuthenticationHandler.SchemeName}")]
public class RagChatController(IMediator mediator) : ControllerBase
{
    [HttpPost("new-session")]
    public async Task<IActionResult> NewSession(RagChatNewSessionCommand request, CancellationToken cancellationToken)
    {
        request.OwnerId = User.GetOwnerId();

        // Which model gets used is now resolved server-side (per customer + feature) inside the
        // handler via IAiModelResolver — the caller no longer names a model.
        request.CustomerId = int.TryParse(User.GetCustomerId(), out var customerId) ? customerId : null;

        var result = await mediator.Send(request, cancellationToken);

        return Ok(result);
    }

    [HttpPost("voice")]
    [RequestSizeLimit(25 * 1024 * 1024)]
    public async Task<IActionResult> Voice(
        [FromForm] RagVoiceChatForm form,
        [FromServices] IRagVoiceChatService voiceService,
        CancellationToken cancellationToken)
    {
        if (form.Audio is null || form.Audio.Length == 0)
            return BadRequest(new { message = "Audio file is required." });

        try
        {
            await using var audioStream = form.Audio.OpenReadStream();

            var result = await voiceService.SendAsync(new RagVoiceChatRequest
            {
                Audio = audioStream,
                FileName = form.Audio.FileName,
                Language = form.Language,
                Voice = form.Voice,
                ConversationId = form.ConversationId,
                DocType = form.DocType,
                Key = form.Key,
                IsMainChat = form.IsMainChat,
                Username = User?.Identity?.Name ?? string.Empty,
                OwnerId = User.GetOwnerId(),
                CustomerId = int.TryParse(User.GetCustomerId(), out var customerId) ? customerId : null
            }, cancellationToken);

            Response.Headers["X-Conversation-Id"] = result.ConversationId.ToString();
            Response.Headers["X-Transcript"] = Uri.EscapeDataString(result.Transcript);
            Response.Headers["X-Response-Text"] = Uri.EscapeDataString(result.ResponseText);
            Response.Headers.AccessControlExposeHeaders = "X-Conversation-Id,X-Transcript,X-Response-Text";

            return File(result.Audio, result.AudioContentType);
        }
        catch (InsufficientCreditException)
        {
            return StatusCode(402, new { message = "Insufficient credit to perform this action." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("send")]
    public async Task<IActionResult> Send([FromBody] RagChatRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await mediator.Send(new RagChatSendCommand
            {
                ConversationId = request.ConversationId,
                Message = request.Message,
                TopK = request.TopK,
                IsMainChat = request.IsMainChat,
                DocType = request.DocType,
                Key = request.Key,
                Username = User?.Identity?.Name ?? string.Empty,
                OwnerId = User.GetOwnerId(),
                CustomerId = int.Parse(User.GetCustomerId())
            }, cancellationToken);

            return Ok(result);
        }
        catch (InsufficientCreditException)
        {
            return StatusCode(402, new { message = "Insufficient credit to perform this action." });
        }
    }
}

public class RagVoiceChatForm
{
    public IFormFile Audio { get; set; } = default!;
    public Guid? ConversationId { get; set; }
    public RagDocType DocType { get; set; } = RagDocType.GeneralChat;
    public string? Key { get; set; }
    public bool IsMainChat { get; set; }
    public string? Language { get; set; }
    public string? Voice { get; set; }
}
