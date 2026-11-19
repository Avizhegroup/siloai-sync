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
    public Task<ResolvedAiModel> ResolveSpeechToTextAsync(int? customerId, Guid? modelId, CancellationToken cancellationToken) =>
        ResolveAsync(customerId, modelId, UsageFeature.SpeechToText, AiModelKind.SpeechToText, cancellationToken);

    public Task<ResolvedAiModel> ResolveTextToSpeechAsync(int? customerId, Guid? modelId, CancellationToken cancellationToken) =>
        ResolveAsync(customerId, modelId, UsageFeature.TextToSpeech, AiModelKind.TextToSpeech, cancellationToken);

    private async Task<ResolvedAiModel> ResolveAsync(
        int? customerId, Guid? modelId, UsageFeature feature, AiModelKind kind, CancellationToken cancellationToken)
    {
        ResolvedAiModel resolved;

        if (modelId is { } id)
        {
            var model = await context.AiModels
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id, cancellationToken)
                ?? throw new InvalidOperationException($"AI model '{id}' was not found.");

            if (!model.IsActive)
                throw new InvalidOperationException($"AI model '{model.Name}' is not active.");

            resolved = AiModelResolver.ToResolved(model);
        }
        else
        {
            resolved = await resolver.ResolveAsync(customerId, feature, cancellationToken);
        }

        return EnsureKind(resolved, kind);
    }

    internal static ResolvedAiModel EnsureKind(ResolvedAiModel model, AiModelKind kind)
    {
        if (model.Kind != kind)
            throw new InvalidOperationException(
                $"Model '{model.Identifier}' is a {model.Kind.ToDisplay()}, but a {kind.ToDisplay()} is required here.");

        return model;
    }
}
