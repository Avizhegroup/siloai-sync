using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SiloAI.Api.Auth;
using SiloAI.Application.Api;
using SiloAI.Shared;
using System.Reflection;
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
