using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SiloAI.Api.Auth;
using SiloAI.Api.Infrastructure;
using SiloAI.Application.Api.Features;

namespace SiloAI.Api.Controllers;

/// <summary>
/// RAG chat endpoints. The caller (customer id, owner key, username) is stamped onto each command by
/// the pipeline behavior and the rules live in the handlers, so this controller only routes.
/// </summary>
[ApiController]
[Route("api/rag/chat")]
[Authorize(AuthenticationSchemes =
    $"{JwtBearerDefaults.AuthenticationScheme},{ApiKeyAuthenticationHandler.SchemeName}")]
public class RagChatController(IMediator mediator) : ControllerBase
{
    [HttpPost("new-session")]
    public async Task<IActionResult> NewSession([FromBody] RagChatNewSessionCommand command, CancellationToken cancellationToken)
        => Ok(await mediator.Send(command, cancellationToken));

    [HttpPost("voice")]
    [RequestSizeLimit(25 * 1024 * 1024)]
    public async Task<IActionResult> Voice([FromForm] RagVoiceChatCommand command, CancellationToken cancellationToken)
        => new VoiceChatActionResult(await mediator.Send(command, cancellationToken));

    [HttpPost("send")]
    public async Task<IActionResult> Send([FromBody] RagChatSendCommand command, CancellationToken cancellationToken)
        => Ok(await mediator.Send(command, cancellationToken));
}
