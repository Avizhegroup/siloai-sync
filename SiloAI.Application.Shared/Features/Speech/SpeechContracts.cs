namespace SiloAI.Application.Shared.Features;

public class TranscriptionResponse
{
    public string Text { get; set; } = string.Empty;
}

public class SynthesizeSpeechRequest
{
    public string? Text { get; set; }

    /// <summary>A voice offered by the chosen model; blank = the model's default (first) voice.</summary>
    public string? Voice { get; set; }

    /// <summary>Admin-only override of the text-to-speech model; null = the assigned model.</summary>
    public Guid? ModelId { get; set; }
}

public static class SpeechLimits
{
    /// <summary>The text-to-speech API accepts at most 4096 characters per request.</summary>
    public const int MaxTextLength = 4096;

    public const long MaxAudioBytes = 25 * 1024 * 1024;

    /// <summary>Suggested voices when creating a text-to-speech model (OpenAI set). Real validation uses the model's own list.</summary>
    public static readonly string[] Voices = ["alloy", "ash", "coral", "echo", "fable", "nova", "onyx", "sage", "shimmer"];
}

public static class SpeechValidation
{
    /// <returns>An error message, or null when the request is valid.</returns>
    public static string? ValidateSynthesis(SynthesizeSpeechRequest? request)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Text))
            return "Text is required.";

        if (request.Text.Length > SpeechLimits.MaxTextLength)
            return $"Text must be at most {SpeechLimits.MaxTextLength} characters.";

        // The voice is validated against the selected model's own voices at synthesis time.
        if (!string.IsNullOrWhiteSpace(request.Voice) && request.Voice.Trim().Length > 60)
            return "Voice name is too long.";

        return null;
    }

    /// <summary>Audio extensions the transcription endpoint understands.</summary>
    public static bool IsSupportedAudioFileName(string? fileName) =>
        Path.GetExtension(fileName ?? string.Empty).ToLowerInvariant()
            is ".mp3" or ".mpeg" or ".mpga" or ".wav" or ".flac" or ".m4a" or ".mp4" or ".ogg" or ".webm";
}

public static class SpeechHeaders
{
    /// <summary>
    /// The voice endpoint returns transcript/response text URL-escaped in response headers
    /// (HTTP headers can't carry raw Persian text). Decodes one such value.
    /// </summary>
    public static string Decode(string? headerValue) =>
        string.IsNullOrEmpty(headerValue) ? string.Empty : Uri.UnescapeDataString(headerValue);
}
