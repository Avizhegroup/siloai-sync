using SiloAI.Application.Api.Services;

namespace SiloAI.Application.Api;

/// <summary>Picks the speech-to-text / text-to-speech model for a call.</summary>
public interface ISpeechModelService
{
    /// <summary>
    /// The model to transcribe with: <paramref name="modelId"/> when given (admin override — must be an
    /// active speech-to-text model), otherwise the customer's / global assignment for the feature.
    /// </summary>
    Task<ResolvedAiModel> ResolveSpeechToTextAsync(int? customerId, Guid? modelId, CancellationToken cancellationToken);

    /// <summary>As <see cref="ResolveSpeechToTextAsync"/>, for text-to-speech; the result carries the model's voices.</summary>
    Task<ResolvedAiModel> ResolveTextToSpeechAsync(int? customerId, Guid? modelId, CancellationToken cancellationToken);
}

public sealed class SpeechModelService(AiApiContext context, IAiModelResolver resolver) : ISpeechModelService
{
    private readonly ModelOverrideService _overrides = new(context, resolver);

    public Task<ResolvedAiModel> ResolveSpeechToTextAsync(int? customerId, Guid? modelId, CancellationToken cancellationToken) =>
        _overrides.ResolveAsync(customerId, modelId, UsageFeature.SpeechToText, AiModelKind.SpeechToText, cancellationToken);

    public Task<ResolvedAiModel> ResolveTextToSpeechAsync(int? customerId, Guid? modelId, CancellationToken cancellationToken) =>
        _overrides.ResolveAsync(customerId, modelId, UsageFeature.TextToSpeech, AiModelKind.TextToSpeech, cancellationToken);
}
