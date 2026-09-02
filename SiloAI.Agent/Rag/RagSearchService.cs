using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.VectorData;
using SiloAI.Application.Shared.Contracts.Rag;
using SiloAI.Domains;
using System.Linq.Expressions;

namespace SiloAI.Agent.Rag;

public class RagSearchService(
    AiApiContext context,
    IEmbeddingService embeddings,
    VectorStoreCollection<Guid, RagDocumentChunk> chunkCollection) : IRagSearchService
{
    public async Task<IReadOnlyList<RagSearchHit>> SearchAsync(
        string query,
        int topK,
        string? docType,
        string? key,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query)) return [];

        var top = Math.Clamp(topK, 1, 100);

        var queryVector = await embeddings.GenerateEmbeddingAsync(query, cancellationToken);

        Expression<Func<RagDocumentChunk, bool>>? filter = null;

        if (!string.IsNullOrWhiteSpace(docType) || !string.IsNullOrWhiteSpace(key))
        {
            var documentIds = await BuildDocumentFilterAsync(docType, key, cancellationToken);
            if (documentIds.Count == 0)
            {
                return [];
            }

            filter = c => documentIds.Contains(c.DocumentId);
        }

        var searchOptions = new VectorSearchOptions<RagDocumentChunk>
        {
            Filter = filter,
            IncludeVectors = false
        };

        var results = chunkCollection.SearchAsync(
            queryVector, top, searchOptions, cancellationToken);

        var hits = new List<RagSearchHit>(top);
        await foreach (var result in results.WithCancellation(cancellationToken))
        {
            var chunk = result.Record;
            var document = await context.RagDocuments
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == chunk.DocumentId, cancellationToken);

            if (document is null)
            {
                continue;
            }

            var distance = result.Score ?? 0d;
            hits.Add(new RagSearchHit(
                ChunkId: chunk.Id,
                DocumentId: chunk.DocumentId,
                FileName: document.OriginalFileName,
                Category: document.Category,
                ChunkIndex: chunk.ChunkIndex,
                Content: chunk.Content,
                Distance: distance,
                Similarity: 1d - distance));
        }

        return hits;
    }

    private async Task<IReadOnlySet<Guid>> BuildDocumentFilterAsync(
        string? docType,
        string? key,
        CancellationToken cancellationToken)
    {
        var query = context.RagDocuments.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(docType))
        {
            query = query.Where(d => d.DocType == docType);
        }

        if (!string.IsNullOrWhiteSpace(key))
        {
            query = query.Where(d => d.Key == key);
        }

        return (await query.Select(d => d.Id).ToListAsync(cancellationToken)).ToHashSet();
    }
}
