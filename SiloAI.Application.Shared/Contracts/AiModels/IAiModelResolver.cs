namespace SiloAI.Application.Shared.Contracts.AiModels;

/// <summary>
/// Resolves which <see cref="AiModel"/> to use for a given call. This is the ONLY place that
/// decides "which model" — replaces the old appsettings-based OpenAI:MainModel / VoiceModel /
/// RagModel / EmbeddingModel configuration, which applied one model to every customer.
/// Resolution order for a feature: the customer's own <see cref="CustomerModelAssignment"/>,
/// then the global default (CustomerId == null) for that feature. Missing configuration is a
/// fail-fast error, not a silent fallback to some hardcoded model.
/// </summary>
public interface IAiModelResolver
{
    /// <summary>Resolve the Normal-kind model assigned to a customer (or the global default) for a feature.</summary>
    Task<ResolvedAiModel> ResolveAsync(int? customerId, UsageFeature feature, CancellationToken cancellationToken);

    /// <summary>The single active Rag-kind (embedding) model shared by all RAG indexing/search.</summary>
    Task<ResolvedAiModel> GetDefaultRagModelAsync(CancellationToken cancellationToken);
}

/// <summary>Just what a caller needs to make the API call, price it, and validate it supports the
/// input/output modality it's about to be used for — not the full entity.</summary>
public record ResolvedAiModel(
    Guid AiModelId,
    string Identifier,
    AiModelKind Kind,
    int? EmbeddingDimensions,
    bool SupportsTextInput,
    bool SupportsTextOutput,
    bool SupportsImageInput,
    bool SupportsImageOutput,
    bool SupportsFileInput,
    bool SupportsFileOutput,
    bool SupportsVoiceInput,
    bool SupportsVoiceOutput,
    decimal InputPricePerMillionTokens,
    decimal OutputPricePerMillionTokens,
    decimal CachedInputPricePerMillionTokens);
