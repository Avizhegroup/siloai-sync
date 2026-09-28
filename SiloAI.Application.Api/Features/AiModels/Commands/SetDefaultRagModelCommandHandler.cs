namespace SiloAI.Application.Api.Features;

public class SetDefaultRagModelCommandHandler(AiApiContext context) : IRequestHandler<SetDefaultRagModelCommand, AiModelDto>
{
    /// <summary>
    /// tbl_RagDocumentChunks.fld_Embedding is a fixed-width VECTOR(1536) column (see the RAG
    /// knowledge-base migration). Switching the active embedding model to one with a different
    /// dimensionality would silently corrupt/break similarity search against every chunk already
    /// indexed with the old model — so it's blocked here rather than left as an admin footgun.
    /// Re-embedding the whole knowledge base and altering the column is a deliberate, separate
    /// operation, not a side effect of picking a different model on this page.
    /// </summary>
    private const int FixedEmbeddingColumnWidth = 1536;

    public async Task<AiModelDto> Handle(SetDefaultRagModelCommand request, CancellationToken cancellationToken)
    {
        var model = await context.AiModels.FirstOrDefaultAsync(m => m.Id == request.AiModelId, cancellationToken)
            ?? throw new InvalidOperationException($"AiModel '{request.AiModelId}' was not found.");

        if (model.Kind != AiModelKind.Rag)
            throw new InvalidOperationException("Only a RAG-kind model can be set as the default RAG model.");

        if (!model.IsActive)
            throw new InvalidOperationException("Cannot set an inactive model as the default RAG model.");

        if (model.EmbeddingDimensions != FixedEmbeddingColumnWidth)
            throw new InvalidOperationException(
                $"'{model.Name}' produces {model.EmbeddingDimensions}-dimension vectors, but the " +
                $"knowledge base's embedding column is fixed at {FixedEmbeddingColumnWidth}. Using it " +
                "would break RAG search for every already-indexed document. Migrating to a different " +
                "vector width requires re-embedding the whole knowledge base — this is not done " +
                "automatically from this page.");

        var currentDefault = await context.AiModels
            .Where(m => m.Kind == AiModelKind.Rag && m.IsDefaultRagModel && m.Id != model.Id)
            .ToListAsync(cancellationToken);

        foreach (var previous in currentDefault)
            previous.IsDefaultRagModel = false;

        model.IsDefaultRagModel = true;
        model.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return UpsertAiModelCommandHandler.ToDto(model);
    }
}
