using SiloAI.Application.Api.Services;

namespace SiloAI.Application.Api;

/// <summary>
/// Lets an admin pick a specific model for a call. With no <c>modelId</c> the model assigned to the
/// feature (customer or global default) is used; either way the model must be of the expected kind.
/// </summary>
public interface IModelOverrideService
{
    Task<ResolvedAiModel> ResolveAsync(
        int? customerId, Guid? modelId, UsageFeature feature, AiModelKind kind, CancellationToken cancellationToken);
}

public sealed class ModelOverrideService(AiApiContext context, IAiModelResolver resolver) : IModelOverrideService
{
    public async Task<ResolvedAiModel> ResolveAsync(
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

        if (resolved.Kind != kind)
            throw new InvalidOperationException(
                $"Model '{resolved.Identifier}' is a {resolved.Kind.ToDisplay()}, but a {kind.ToDisplay()} is required here.");

        return resolved;
    }
}
