using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SiloAI.Api.Auth;
using SiloAI.Application.Api.Features;

namespace SiloAI.Api.Controllers;

/// <summary>
/// Instructions + optional text + images in, a text analysis out. JWT (admin panel) or API key
/// (customers), same as the RAG chat endpoints; the customer, when present, is billed through the ledger.
/// </summary>
[ApiController]
[Route("api/image-analysis")]
[Authorize(AuthenticationSchemes =
    $"{JwtBearerDefaults.AuthenticationScheme},{ApiKeyAuthenticationHandler.SchemeName}")]
public class ImageAnalysisController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    [RequestSizeLimit(ImageUploadLimits.MaxRequestBytes)]
    public async Task<IActionResult> Analyze([FromForm] AnalyzeImageFormCommand command, CancellationToken cancellationToken)
        => Ok(await mediator.Send(command, cancellationToken));
}
