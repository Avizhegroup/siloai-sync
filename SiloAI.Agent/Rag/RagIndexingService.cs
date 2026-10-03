using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.VectorData;
using SiloAI.Application.Shared.Contracts.Rag;
using SiloAI.Domains;

namespace SiloAI.Agent.Rag;

public class RagIndexingService(
    AiApiContext context,
    ITextExtractionDispatcher extractor,
    ITextChunkingService chunker,
    IEmbeddingService embeddings,
    VectorStoreCollection<Guid, RagDocumentChunk> chunkCollection,
    ILogger<RagIndexingService> logger) : IRagIndexingService
{
    public async Task<RagIndexingResult> IndexAsync(Guid documentId, Stream content, string fileName, string contentType, CancellationToken cancellationToken)
    {
        var document = await context.RagDocuments
            .FirstOrDefaultAsync(d => d.Id == documentId, cancellationToken)
            ?? throw new InvalidOperationException($"RagDocument {documentId} not found.");

        return await RunPipelineAsync(document, content, fileName, contentType, deleteExisting: false, cancellationToken);
    }

    public async Task<RagIndexingResult> RebuildAsync(Guid documentId, Stream content, string fileName, string contentType, CancellationToken cancellationToken)
    {
        var document = await context.RagDocuments
            .FirstOrDefaultAsync(d => d.Id == documentId, cancellationToken)
            ?? throw new InvalidOperationException($"RagDocument {documentId} not found.");

        return await RunPipelineAsync(document, content, fileName, contentType, deleteExisting: true, cancellationToken);
    }

    private async Task<RagIndexingResult> RunPipelineAsync(
        RagDocument document,
        Stream content,
        string fileName,
        string contentType,
        bool deleteExisting,
        CancellationToken cancellationToken)
    {
        try
        {
            document.ProcessingStatus = RagProcessingStatus.Processing;
            document.ProcessingError = null;
            document.LastUpdateDateTime = DateTime.Now;
            await context.SaveChangesAsync(cancellationToken);

            if (deleteExisting)
            {
                await context.RagDocumentChunks
                    .Where(c => c.DocumentId == document.Id)
                    .ExecuteDeleteAsync(cancellationToken);
            }

            var text = await extractor.ExtractTextAsync(content, fileName, contentType, cancellationToken);

            var chunks = chunker.CreateChunks(text);

            if (chunks.Count == 0)
            {
                document.ProcessingStatus = RagProcessingStatus.Completed;
                document.ChunkCount = 0;
                document.LastUpdateDateTime = DateTime.Now;
                await context.SaveChangesAsync(cancellationToken);
                return new RagIndexingResult(document.Id, 0, document.ProcessingStatus);
            }

            var vectors = await embeddings.GenerateEmbeddingsAsync(
                chunks.Select(c => c.Content).ToList(),
                cancellationToken);

            if (vectors.Count != chunks.Count)
            {
                throw new InvalidOperationException(
                    $"Embedding count mismatch (expected {chunks.Count}, got {vectors.Count}).");
            }

            var now = DateTime.Now;
            var entities = chunks.Select((c, i) => new RagDocumentChunk
            {
                Id = Guid.NewGuid(),
                DocumentId = document.Id,
                ChunkIndex = c.Index,
                Content = c.Content,
                TokenCount = c.TokenCount,
                CreateDateTime = now,
                Embedding = vectors[i]
            }).ToList();

            context.RagDocumentChunks.AddRange(entities);
            await context.SaveChangesAsync(cancellationToken);

            // Write the embeddings (and the rest of each row) through the same
            // VectorStoreCollection abstraction RagSearchService already uses for reads,
            // instead of raw SQL casting a JSON-array literal to VECTOR(N). EF Core still owns
            // the non-vector columns above; this call is responsible only for getting
            // fld_Embedding populated on the SQL Server 2025 native VECTOR column.
            await chunkCollection.UpsertAsync(entities, cancellationToken);

            document.ProcessingStatus = RagProcessingStatus.Completed;
            document.ChunkCount = entities.Count;
            document.LastUpdateDateTime = DateTime.Now;
            await context.SaveChangesAsync(cancellationToken);

            return new RagIndexingResult(document.Id, entities.Count, document.ProcessingStatus);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "RAG indexing failed for document {DocumentId}", document.Id);

            document.ProcessingStatus = RagProcessingStatus.Failed;
            document.ProcessingError = Truncate(ex.Message, 2000);
            document.LastUpdateDateTime = DateTime.Now;
            await context.SaveChangesAsync(CancellationToken.None);

            return new RagIndexingResult(document.Id, document.ChunkCount, document.ProcessingStatus, ex.Message);
        }
    }

    private static string Truncate(string value, int max) =>
        string.IsNullOrEmpty(value) || value.Length <= max ? value : value[..max];
}
