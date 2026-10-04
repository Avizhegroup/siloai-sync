using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.VectorData;
using SiloAI.Application.Shared.Contracts.Rag;
using SiloAI.Application.Shared.Features;
using SiloAI.Domains;
using System.Linq.Expressions;

namespace SiloAI.Agent.Rag;

public class RagSearchService(
    AiApiContext context,
    IEmbeddingService embeddings,
    VectorStoreCollection<Guid, RagDocumentChunk> chunkCollection,
    IOptions<RagOptions> ragOptions) : IRagSearchService
{
    private static readonly char[] WordSeparators =
        [' ', '\t', '\n', '\r', '،', '.', ',', '؟', '?', '!', ':', ';'];

    public async Task<IReadOnlyList<RagSearchHit>> SearchAsync(
        string query,
        int topK,
        RagDocType? docType,
        string? key,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query)) return [];

        var top = Math.Clamp(topK, 1, 100);

        var options = ragOptions.Value;
        var candidateCount = Math.Clamp(top * Math.Max(1, options.HybridCandidateMultiplier), top, 100);
        var keywordWeight = Math.Clamp(options.HybridKeywordWeight, 0d, 1d);
        var semanticWeight = 1d - keywordWeight;

        var queryVector = await embeddings.GenerateEmbeddingAsync(query, cancellationToken);
        var queryWords = SplitWords(query);

        Expression<Func<RagDocumentChunk, bool>>? filter = null;

        if (docType.HasValue || !string.IsNullOrWhiteSpace(key))
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

        // Retrieve a wider semantic candidate pool than requested, so the keyword signal below
        // has something to rerank — the standard retrieve-then-rerank pattern.
        var results = chunkCollection.SearchAsync(
            queryVector, candidateCount, searchOptions, cancellationToken);

        var chunkResults = new List<(RagDocumentChunk Chunk, double Distance)>(candidateCount);
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

        // Blend semantic similarity with keyword overlap, then rerank candidates by the blended
        // score before taking the top K — pure vector distance alone misses exact terms
        // (codes, names) that keyword matching catches.
        var ranked = new List<(RagDocumentChunk Chunk, RagDocument Document, double Distance, double Similarity, double FinalScore)>(chunkResults.Count);
        foreach (var (chunk, distance) in chunkResults)
        {
            if (!documentsById.TryGetValue(chunk.DocumentId, out var document))
            {
                continue;
            }

            var similarity = 1d - distance;
            var keywordScore = ComputeKeywordScore(queryWords, chunk.Content);
            var finalScore = (semanticWeight * similarity) + (keywordWeight * keywordScore);

            ranked.Add((chunk, document, distance, similarity, finalScore));
        }

        var hits = ranked
            .OrderByDescending(r => r.FinalScore)
            .Take(top)
            .Select(r => new RagSearchHit(
                ChunkId: r.Chunk.Id,
                DocumentId: r.Chunk.DocumentId,
                FileName: r.Document.OriginalFileName,
                Category: r.Document.Category,
                ChunkIndex: r.Chunk.ChunkIndex,
                Content: r.Chunk.Content,
                Distance: r.Distance,
                Similarity: r.Similarity))
            .ToList();

        return hits;
    }

    private static string[] SplitWords(string text) =>
        text.Split(WordSeparators, StringSplitOptions.RemoveEmptyEntries)
            .Where(w => w.Length > 1)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

    /// <summary>
    /// Fraction (0–1) of the query's distinct words that appear in the chunk's content.
    /// Deliberately simple substring matching (not regex word boundaries, which don't work
    /// reliably across Persian script) — consistent with the LIKE N'%word%' per-word matching
    /// convention already used elsewhere in this codebase for Persian text.
    /// </summary>
    private static double ComputeKeywordScore(string[] queryWords, string content)
    {
        if (queryWords.Length == 0 || string.IsNullOrEmpty(content))
        {
            return 0d;
        }

        var matched = queryWords.Count(w => content.Contains(w, StringComparison.OrdinalIgnoreCase));

        return (double)matched / queryWords.Length;
    }

    private async Task<List<Guid>> BuildDocumentFilterAsync(
        RagDocType? docType,
        string? key,
        CancellationToken cancellationToken)
    {
        var query = context.RagDocuments.AsNoTracking().AsQueryable();

        if (docType.HasValue)
        {
            // RagDocument.DocType is a string column storing the enum's *name*
            // (see UploadRagDocumentCommandHandler: DocType = request.DocType.ToString()),
            // not its ordinal value — comparing against ((int)docType.Value).ToString() here
            // never matched anything, silently filtering every document out of every
            // DocType-scoped search (which is what RagChatSendHandler always performs, since
            // RagChatSendCommand.DocType is a non-nullable RagDocType with a default, so
            // docType.HasValue is always true for chat).
            var docTypeName = docType.Value.ToString();
            query = query.Where(d => d.DocType == docTypeName);
        }

        if (!string.IsNullOrWhiteSpace(key))
        {
            query = query.Where(d => d.Key == key);
        }

        return await query.Select(d => d.Id).Distinct().ToListAsync(cancellationToken);
    }
}
