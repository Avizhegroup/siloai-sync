using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Embeddings;
using SiloAI.Application.Shared.Contracts.Rag;
using SiloAI.Domains;
using System.ClientModel;

namespace SiloAI.Agent.Rag;

/// <summary>
/// <see cref="IEmbeddingService"/> backed by the official OpenAI .NET client. Supports a
/// custom endpoint (Azure OpenAI / GitHub Models gateway) via <see cref="OpenAIOptions.Endpoint"/>.
/// Which embedding model to call (and its vector width) is resolved from the single active
/// tbl_AiModels row (Kind == Rag, IsDefaultRagModel == true) — not from appsettings — via
/// AiApiContext, resolved once per scope in the constructor. A synchronous read here (rather
/// than an async factory) is deliberate: this type is Scoped and Kestrel has no
/// SynchronizationContext to deadlock on, so a one-time, lightweight query at construction is
/// safe and keeps <see cref="ModelName"/>/<see cref="Dimensions"/> usable as plain sync
/// properties for existing callers (RagIndexingService, RagDocumentChunkCollectionOptionsFactory)
/// without changing the IEmbeddingService contract.
/// </summary>
public class OpenAIEmbeddingService : IEmbeddingService
{
    private readonly OpenAIOptions _options;
    private readonly EmbeddingClient _client;

    public OpenAIEmbeddingService(IOptions<OpenAIOptions> options, AiApiContext context)
    {
        _options = options.Value;

        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new InvalidOperationException("OpenAI:ApiKey is not configured.");
        }

        var ragModel = context.AiModels
            .AsNoTracking()
            .Where(m => m.Kind == AiModelKind.Rag && m.IsDefaultRagModel && m.IsActive)
            .Select(m => new { m.Identifier, m.EmbeddingDimensions })
            .FirstOrDefault();

        if (ragModel is null)
        {
            throw new InvalidOperationException(
                "No active RAG (embedding) model is configured. Mark one as the default RAG " +
                "model on the AI Models admin page before indexing or searching the knowledge base.");
        }

        ModelName = ragModel.Identifier;
        Dimensions = ragModel.EmbeddingDimensions
            ?? throw new InvalidOperationException(
                $"AiModel '{ragModel.Identifier}' is marked as the default RAG model but has no " +
                "EmbeddingDimensions set.");

        var credential = new ApiKeyCredential(_options.ApiKey);
        var clientOptions = new OpenAIClientOptions();
        if (!string.IsNullOrWhiteSpace(_options.Endpoint))
        {
            clientOptions.Endpoint = new Uri(_options.Endpoint);
        }

        _client = new EmbeddingClient(ModelName, credential, clientOptions);
    }

    public string ModelName { get; }

    public int Dimensions { get; }

    public async Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken)
    {
        var result = await _client.GenerateEmbeddingAsync(text, cancellationToken: cancellationToken);
        return result.Value.ToFloats().ToArray();
    }

    public async Task<IReadOnlyList<float[]>> GenerateEmbeddingsAsync(IReadOnlyList<string> texts, CancellationToken cancellationToken)
    {
        if (texts is null || texts.Count == 0) return [];

        var result = await _client.GenerateEmbeddingsAsync(texts, cancellationToken: cancellationToken);
        return [.. result.Value.Select(e => e.ToFloats().ToArray())];
    }
}
