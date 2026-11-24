using System.ClientModel;
using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Audio;
using SiloAI.Agent.Rag;

namespace SiloAI.Application.Api;

public sealed class OpenAISpeechToTextService : ISpeechToTextService
{
    private readonly IOptions<OpenAIOptions> _options;

    public OpenAISpeechToTextService(IOptions<OpenAIOptions> options)
    {
        _options = options;
    }

    public async Task<string> TranscribeAsync(
        Stream audio, string fileName, string? language, ResolvedAiModel model, CancellationToken cancellationToken)
        => (await TranscribeWithUsageAsync(audio, fileName, language, model, cancellationToken)).Text;

    public async Task<TranscriptionResult> TranscribeWithUsageAsync(
        Stream audio, string fileName, string? language, ResolvedAiModel model, CancellationToken cancellationToken)
    {
        //var transcriptionOptions = new AudioTranscriptionOptions();
        //if (!string.IsNullOrWhiteSpace(language))
        //    transcriptionOptions.Language = language;

        //audio.Position = 0;

        //var result = await _client.TranscribeAudioAsync(audio, fileName, transcriptionOptions, cancellationToken);


        var baseUrl = _options.Value.Endpoint.TrimEnd('/');
        if (!baseUrl.EndsWith("/v1", StringComparison.OrdinalIgnoreCase))
            baseUrl += "/v1";

        using var client = new HttpClient { BaseAddress = new Uri(baseUrl + "/") };
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _options.Value.ApiKey);

        using var form = new MultipartFormDataContent();
        
        form.Add(new StringContent(model.Identifier), "model");
       
        if (!string.IsNullOrWhiteSpace(language))
            form.Add(new StringContent(language), "language");

        var contentType = Path.GetExtension(fileName).ToLowerInvariant() switch
        {
            ".mp3" or ".mpeg" or ".mpga" => "audio/mpeg",
            ".flac" => "audio/flac",
            ".m4a" or ".mp4" => "audio/mp4",
            ".ogg" => "audio/ogg",
            ".webm" => "audio/webm",
            _ => "audio/wav"
        };

        if (audio.CanSeek)
            audio.Position = 0;

        var audioContent = new StreamContent(audio);
        audioContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        form.Add(audioContent, "file", string.IsNullOrWhiteSpace(fileName) ? "audio.wav" : fileName);

        using var response = await client.PostAsync("audio/transcriptions", form, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        response.EnsureSuccessStatusCode();

        using var doc = System.Text.Json.JsonDocument.Parse(body);
        var text = doc.RootElement.TryGetProperty("text", out var t) ? t.GetString() ?? string.Empty : string.Empty;
        return new TranscriptionResult(text, TranscriptionUsageParser.Parse(doc.RootElement));
    }
}

/// <summary>
/// Reads the consumption an OpenAI-compatible transcription response reports: either
/// <c>usage: {type:"tokens", input_tokens, output_tokens}</c>, <c>usage: {type:"duration", seconds}</c>,
/// or a top-level <c>duration</c> (verbose_json). Anything missing is zero.
/// </summary>
public static class TranscriptionUsageParser
{
    public static SpeechUsage Parse(System.Text.Json.JsonElement root)
    {
        long input = 0, output = 0;
        decimal seconds = 0m;

        if (root.TryGetProperty("usage", out var usage) && usage.ValueKind == System.Text.Json.JsonValueKind.Object)
        {
            if (usage.TryGetProperty("input_tokens", out var i) && i.TryGetInt64(out var iv)) input = iv;
            if (usage.TryGetProperty("output_tokens", out var o) && o.TryGetInt64(out var ov)) output = ov;
            if (usage.TryGetProperty("seconds", out var s) && s.TryGetDecimal(out var sv)) seconds = sv;
        }

        if (seconds == 0m && root.TryGetProperty("duration", out var d) && d.ValueKind == System.Text.Json.JsonValueKind.Number && d.TryGetDecimal(out var dv))
            seconds = dv;

        return new SpeechUsage(input, output, seconds);
    }
}

public sealed class OpenAITextToSpeechService : ITextToSpeechService
{
    private readonly IOptions<OpenAIOptions> _options;

    public OpenAITextToSpeechService(IOptions<OpenAIOptions> options)
    {
        _options = options;
    }

    public async Task<SpeechResult> SynthesizeAsync(
        string text, string? voice, ResolvedAiModel model, CancellationToken cancellationToken)
    {
        var client = new AudioClient(
            model.Identifier, OpenAIClientFactory.Credential(_options.Value), OpenAIClientFactory.ClientOptions(_options.Value));

        // The voice must be one this model offers (blank = the model's first/default voice).
        var chosenVoice = AiModelVoices.Choose(model.VoiceList, voice);

        var result = await client.GenerateSpeechAsync(
            text,
            new GeneratedSpeechVoice(chosenVoice),
            new SpeechGenerationOptions { ResponseFormat = GeneratedSpeechFormat.Mp3 },
            cancellationToken);

        return new SpeechResult(result.Value.ToArray(), "audio/mpeg");
    }
}

internal static class OpenAIClientFactory
{
    public static ApiKeyCredential Credential(OpenAIOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.ApiKey))
            throw new InvalidOperationException("OpenAI:ApiKey is not configured.");

        return new ApiKeyCredential(options.ApiKey);
    }

    public static OpenAIClientOptions ClientOptions(OpenAIOptions options)
    {
        var clientOptions = new OpenAIClientOptions();
        if (!string.IsNullOrWhiteSpace(options.Endpoint))
            clientOptions.Endpoint = new Uri(options.Endpoint);

        return clientOptions;
    }
}
