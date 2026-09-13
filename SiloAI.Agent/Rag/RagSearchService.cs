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
            var documentIdsFiltered = await BuildDocumentFilterAsync(docType, key, cancellationToken);
           
            if (documentIdsFiltered.Count == 0)
            {
                return [];
            }

            filter = c => documentIdsFiltered.Contains(c.DocumentId);
        }

        var searchOptions = new VectorSearchOptions<RagDocumentChunk>
        {
            Filter = filter,
            IncludeVectors = false
        };

        var results = chunkCollection.SearchAsync(
            queryVector, top, searchOptions, cancellationToken);

        var chunkResults = new List<(RagDocumentChunk Chunk, double Distance)>(top);
        await foreach (var result in results.WithCancellation(cancellationToken))
        {
            chunkResults.Add((result.Record, result.Score ?? 0d));
        }

        if (chunkResults.Count == 0)
        {
            return [];
        }

        // Single batched lookup for FileName/Category instead of one query per hit.
        var documentIds = chunkResults.Select(r => r.Chunk.DocumentId).Distinct().ToList();
        var documentsById = (await context.RagDocuments
                .AsNoTracking()
                .Where(d => documentIds.Contains(d.Id))
                .ToListAsync(cancellationToken))
            .ToDictionary(d => d.Id);

        var hits = new List<RagSearchHit>(chunkResults.Count);
        foreach (var (chunk, distance) in chunkResults)
        {
            if (!documentsById.TryGetValue(chunk.DocumentId, out var document))
            {
                continue;
            }

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

    private async Task<List<Guid>> BuildDocumentFilterAsync(
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

        return await query.Select(d => d.Id).Distinct().ToListAsync(cancellationToken);
    }
}
