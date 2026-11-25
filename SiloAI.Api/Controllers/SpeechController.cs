using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SiloAI.Application.Api.Features;

namespace SiloAI.Api.Controllers;

/// <summary>
/// Standalone speech-to-text and text-to-speech. JWT only on purpose (not exposed to customer API
/// keys). Calls made by a customer are metered and charged to their credit; admin calls (no
/// customer id) stay unmetered. All rules live in the command handlers.
/// </summary>
[ApiController]
[Route("api/speech")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class SpeechController(IMediator mediator) : ControllerBase
{
    [HttpPost("transcribe")]
    [RequestSizeLimit(SpeechLimits.MaxAudioBytes)]
    public async Task<IActionResult> Transcribe([FromForm] TranscribeSpeechCommand command, CancellationToken cancellationToken)
        => Ok(await mediator.Send(command, cancellationToken));

    [HttpPost("synthesize")]
    public async Task<IActionResult> Synthesize([FromBody] SynthesizeSpeechCommand command, CancellationToken cancellationToken)
    {
        var speech = await mediator.Send(command, cancellationToken);

        return File(speech.Audio, speech.ContentType);
    }
}
