using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SiloAI.Api.Auth;
using SiloAI.Application.Api.Features;

namespace SiloAI.Api.Controllers;

[ApiController]
[Route("api/image")]
[Authorize(AuthenticationSchemes = ApiKeyAuthenticationHandler.SchemeName)]
public class ImageController(IMediator mediator) : ControllerBase
{
    [HttpPost("ocr")]
    public async Task<IActionResult> Ocr([FromForm] OcrFormCommand command, CancellationToken cancellationToken)
        => Ok(await mediator.Send(command, cancellationToken));
}
