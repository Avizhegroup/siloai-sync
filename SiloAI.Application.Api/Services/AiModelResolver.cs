using SiloAI.Application.Shared.Contracts.AiModels;

namespace SiloAI.Application.Api.Services;

/// <inheritdoc cref="IAiModelResolver"/>
public class AiModelResolver(AiApiContext dbContext) : IAiModelResolver
{
    public async Task<ResolvedAiModel> ResolveAsync(int? customerId, UsageFeature feature, CancellationToken cancellationToken)
    {
        // 1) The customer's own override, if any.
        if (customerId is { } id)
        {
            var customerModel = await dbContext.CustomerModelAssignments
                .AsNoTracking()
                .Where(a => a.CustomerId == id && a.Feature == feature)
                .Select(a => a.AiModel)
                .FirstOrDefaultAsync(cancellationToken);

            if (customerModel is not null)
                return ToResolved(customerModel);
        }

        // 2) The global default (CustomerId == null) for this feature.
        var defaultModel = await dbContext.CustomerModelAssignments
            .AsNoTracking()
            .Where(a => a.CustomerId == null && a.Feature == feature)
            .Select(a => a.AiModel)
            .FirstOrDefaultAsync(cancellationToken);

        if (defaultModel is not null)
            return ToResolved(defaultModel);

        throw new InvalidOperationException(
            $"No AI model is configured for feature '{feature}' (no per-customer assignment and no " +
            "global default). Configure one on the AI Models admin page before this feature can be used.");
    }

    public async Task<ResolvedAiModel> GetDefaultRagModelAsync(CancellationToken cancellationToken)
    {
        var model = await dbContext.AiModels
            .AsNoTracking()
            .Where(m => m.Kind == AiModelKind.Rag && m.IsDefaultRagModel && m.IsActive)
            .FirstOrDefaultAsync(cancellationToken);

        if (model is null)
            throw new InvalidOperationException(
                "No active RAG (embedding) model is configured. Mark one as the default RAG model on " +
                "the AI Models admin page before indexing or searching the knowledge base.");

        return ToResolved(model);
    }

    private static ResolvedAiModel ToResolved(AiModel model) => new(
        model.Id,
        model.Identifier,
        model.Kind,
        model.EmbeddingDimensions,
        model.SupportsTextInput,
        model.SupportsTextOutput,
        model.SupportsImageInput,
        model.SupportsImageOutput,
        model.SupportsFileInput,
        model.SupportsFileOutput,
        model.SupportsVoiceInput,
        model.SupportsVoiceOutput,
        model.InputPricePerMillionTokens,
        model.OutputPricePerMillionTokens,
        model.CachedInputPricePerMillionTokens);
}
