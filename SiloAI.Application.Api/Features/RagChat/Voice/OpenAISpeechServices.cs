using System.ClientModel;
using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Audio;
using SiloAI.Agent.Rag;

namespace SiloAI.Application.Api;

public sealed class OpenAISpeechToTextService : ISpeechToTextService
{
    public const string Model = "microsoft/mai-transcribe-2";
    private readonly AudioClient _client;
    private IOptions<OpenAIOptions> _options;

    public OpenAISpeechToTextService(IOptions<OpenAIOptions> options)
    {
        _options = options;
        _client = new AudioClient(Model, OpenAIClientFactory.Credential(options.Value), OpenAIClientFactory.ClientOptions(options.Value));
    }

    public async Task<string> TranscribeAsync(Stream audio, string fileName, string? language, CancellationToken cancellationToken)
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
        
        form.Add(new StringContent(Model), "model");
       
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
        return doc.RootElement.TryGetProperty("text", out var text) ? text.GetString() ?? string.Empty : string.Empty;
    }
}

public sealed class OpenAITextToSpeechService : ITextToSpeechService
{
    public const string Model = "openai/tts-1-hd";
    private const string DefaultVoice = "alloy";

    private readonly AudioClient _client;

    public OpenAITextToSpeechService(IOptions<OpenAIOptions> options)
    {
        _client = new AudioClient(Model, OpenAIClientFactory.Credential(options.Value), OpenAIClientFactory.ClientOptions(options.Value));
    }

    public async Task<SpeechResult> SynthesizeAsync(string text, string? voice, CancellationToken cancellationToken)
    {
        var result = await _client.GenerateSpeechAsync(
            text,
            new GeneratedSpeechVoice(string.IsNullOrWhiteSpace(voice) ? DefaultVoice : voice),
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
