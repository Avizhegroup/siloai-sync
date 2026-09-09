using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SiloAI.Agent.Rag;
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
public class RagChatController(
    IMediator mediator,
    IOptions<OpenAIOptions> openAiOptions) : ControllerBase
{
    [HttpPost("new-session")]
    public async Task<IActionResult> NewSession(RagChatNewSessionCommand request, CancellationToken cancellationToken)
    {
        request.RagModel = openAiOptions.Value.RagModel;

        request.OwnerId = User.GetOwnerId();

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
                RagModel = openAiOptions.Value.RagModel,
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
