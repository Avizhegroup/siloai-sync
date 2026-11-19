using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SiloAI.Api.Auth;
using SiloAI.Application.Api;
using SiloAI.Application.Shared.Features;

namespace SiloAI.Api.Controllers;

public class TranscribeForm
{
    public IFormFile Audio { get; set; } = default!;
    public string? Language { get; set; }

    /// <summary>Admin-only override of the speech-to-text model; null = the assigned model.</summary>
    public Guid? ModelId { get; set; }
}

/// <summary>
/// Standalone speech-to-text and text-to-speech for the admin panel's speech page. JWT only on
/// purpose: these calls are not metered per customer (the voice-chat endpoint isn't either), so
/// they are not exposed to customer API keys.
/// </summary>
[ApiController]
[Route("api/speech")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class SpeechController : ControllerBase
{
    [HttpPost("transcribe")]
    [RequestSizeLimit(SpeechLimits.MaxAudioBytes)]
    public async Task<IActionResult> Transcribe(
        [FromForm] TranscribeForm form,
        [FromServices] ISpeechToTextService stt,
        [FromServices] ISpeechModelService speechModels,
        CancellationToken cancellationToken)
    {
        var customerId = int.TryParse(User.GetCustomerId(), out var cid) ? cid : (int?)null;

        // Choosing a model is an admin (no customer) capability; customers get their assigned one.
        if (form.ModelId is not null && customerId is not null)
            return StatusCode(403, new { message = "Customers cannot choose the speech model." });

        if (form.Audio is null || form.Audio.Length == 0)
            return BadRequest(new { message = "Audio file is required." });

        if (!SpeechValidation.IsSupportedAudioFileName(form.Audio.FileName))
            return BadRequest(new { message = "Unsupported audio type. Use mp3, wav, flac, m4a, mp4, ogg or webm." });

        try
        {
            await using var stream = form.Audio.OpenReadStream();

            var model = await speechModels.ResolveSpeechToTextAsync(customerId, form.ModelId, cancellationToken);

            var text = (await stt.TranscribeAsync(stream, form.Audio.FileName, form.Language, model, cancellationToken)).Trim();

            return Ok(new TranscriptionResponse { Text = text });
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(502, new { message = $"Speech-to-text provider error: {ex.Message}" });
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(422, new { message = ex.Message });
        }
    }

    [HttpPost("synthesize")]
    public async Task<IActionResult> Synthesize(
        [FromBody] SynthesizeSpeechRequest request,
        [FromServices] ITextToSpeechService tts,
        [FromServices] ISpeechModelService speechModels,
        CancellationToken cancellationToken)
    {
        var customerId = int.TryParse(User.GetCustomerId(), out var cid) ? cid : (int?)null;

        if (request?.ModelId is not null && customerId is not null)
            return StatusCode(403, new { message = "Customers cannot choose the speech model." });

        var error = SpeechValidation.ValidateSynthesis(request);
        if (error is not null)
            return BadRequest(new { message = error });

        try
        {
            var model = await speechModels.ResolveTextToSpeechAsync(customerId, request.ModelId, cancellationToken);

            var speech = await tts.SynthesizeAsync(request.Text!.Trim(), request.Voice?.Trim(), model, cancellationToken);

            return File(speech.Audio, speech.ContentType);
        }
        catch (Exception ex) when (ex is System.ClientModel.ClientResultException or HttpRequestException)
        {
            return StatusCode(502, new { message = $"Text-to-speech provider error: {ex.Message}" });
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(422, new { message = ex.Message });
        }
    }
}
