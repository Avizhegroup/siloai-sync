using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SiloAI.Application.Api;
using SiloAI.Application.Shared.Features;

namespace SiloAI.Api.Controllers;

public class TranscribeForm
{
    public IFormFile Audio { get; set; } = default!;
    public string? Language { get; set; }
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
        CancellationToken cancellationToken)
    {
        if (form.Audio is null || form.Audio.Length == 0)
            return BadRequest(new { message = "Audio file is required." });

        if (!SpeechValidation.IsSupportedAudioFileName(form.Audio.FileName))
            return BadRequest(new { message = "Unsupported audio type. Use mp3, wav, flac, m4a, mp4, ogg or webm." });

        try
        {
            await using var stream = form.Audio.OpenReadStream();

            var text = (await stt.TranscribeAsync(stream, form.Audio.FileName, form.Language, cancellationToken)).Trim();

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
        CancellationToken cancellationToken)
    {
        var error = SpeechValidation.ValidateSynthesis(request);
        if (error is not null)
            return BadRequest(new { message = error });

        try
        {
            var speech = await tts.SynthesizeAsync(request.Text!.Trim(), request.Voice?.Trim().ToLowerInvariant(), cancellationToken);

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
