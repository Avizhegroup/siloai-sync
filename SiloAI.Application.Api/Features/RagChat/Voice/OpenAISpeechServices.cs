using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Audio;
using SiloAI.Agent.Rag;
using System.ClientModel;

namespace SiloAI.Application.Api;

public sealed class OpenAISpeechToTextService : ISpeechToTextService
{
    public const string Model = "microsoft/mai-transcribe-2";

    private readonly AudioClient _client;

    public OpenAISpeechToTextService(IOptions<OpenAIOptions> options)
    {
        _client = new AudioClient(Model, OpenAIClientFactory.Credential(options.Value), OpenAIClientFactory.ClientOptions(options.Value));
    }

    public async Task<string> TranscribeAsync(Stream audio, string fileName, string? language, CancellationToken cancellationToken)
    {
        var transcriptionOptions = new AudioTranscriptionOptions();
        if (!string.IsNullOrWhiteSpace(language))
            transcriptionOptions.Language = language;

        audio.Position = 0;

        var result = await _client.TranscribeAudioAsync(audio, fileName, transcriptionOptions, cancellationToken);
        return result.Value.Text ?? string.Empty;
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
