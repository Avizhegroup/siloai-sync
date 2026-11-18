namespace SiloAI.Application.Shared.Features;

public class TranscriptionResponse
{
    public string Text { get; set; } = string.Empty;
}

public class SynthesizeSpeechRequest
{
    public string? Text { get; set; }

    /// <summary>One of <see cref="SpeechLimits.Voices"/>; blank = the service default.</summary>
    public string? Voice { get; set; }
}

public static class SpeechLimits
{
    /// <summary>The text-to-speech API accepts at most 4096 characters per request.</summary>
    public const int MaxTextLength = 4096;

    public const long MaxAudioBytes = 25 * 1024 * 1024;

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

        if (!string.IsNullOrWhiteSpace(request.Voice)
            && !SpeechLimits.Voices.Contains(request.Voice.Trim(), StringComparer.OrdinalIgnoreCase))
            return $"Unknown voice '{request.Voice}'.";

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
