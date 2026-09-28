namespace SiloAI.Application.Api.Features;

public class UpsertAiModelCommandHandler(AiApiContext context) : IRequestHandler<UpsertAiModelCommand, AiModelDto>
{
    public async Task<AiModelDto> Handle(UpsertAiModelCommand request, CancellationToken cancellationToken)
    {
        var input = request.Model;

        if (input.Kind == AiModelKind.Rag && (input.EmbeddingDimensions is null || input.EmbeddingDimensions <= 0))
            throw new InvalidOperationException("A RAG (embedding) model requires a positive EmbeddingDimensions value.");

        var duplicateIdentifier = await context.AiModels
            .AsNoTracking()
            .AnyAsync(m => m.Identifier == input.Identifier && m.Id != input.Id, cancellationToken);

        if (duplicateIdentifier)
            throw new InvalidOperationException($"A model with identifier '{input.Identifier}' already exists.");

        var now = DateTime.UtcNow;
        AiModel model;

        if (input.Id == Guid.Empty)
        {
            model = new AiModel { Id = Guid.NewGuid(), CreatedAt = now };
            context.AiModels.Add(model);
        }
        else
        {
            model = await context.AiModels.FirstOrDefaultAsync(m => m.Id == input.Id, cancellationToken)
                ?? throw new InvalidOperationException($"AiModel '{input.Id}' was not found.");
        }

        model.Name = input.Name;
        model.Identifier = input.Identifier;
        model.Kind = input.Kind;

        // Capability flags and embedding dimensions only make sense for their own Kind — clear
        // the irrelevant side rather than trust the client not to send stale values from a
        // previous edit (e.g. flipping a model from Normal to Rag or back).
        if (input.Kind == AiModelKind.Normal)
        {
            model.SupportsTextInput = input.SupportsTextInput;
            model.SupportsTextOutput = input.SupportsTextOutput;
            model.SupportsImageInput = input.SupportsImageInput;
            model.SupportsImageOutput = input.SupportsImageOutput;
            model.SupportsFileInput = input.SupportsFileInput;
            model.SupportsFileOutput = input.SupportsFileOutput;
            model.SupportsVoiceInput = input.SupportsVoiceInput;
            model.SupportsVoiceOutput = input.SupportsVoiceOutput;
            model.EmbeddingDimensions = null;
            model.IsDefaultRagModel = false;
        }
        else
        {
            model.SupportsTextInput = false;
            model.SupportsTextOutput = false;
            model.SupportsImageInput = false;
            model.SupportsImageOutput = false;
            model.SupportsFileInput = false;
            model.SupportsFileOutput = false;
            model.SupportsVoiceInput = false;
            model.SupportsVoiceOutput = false;
            model.EmbeddingDimensions = input.EmbeddingDimensions;
            // IsDefaultRagModel is intentionally left untouched here — toggled only via
            // SetDefaultRagModelCommand, which also enforces the single-default invariant.
        }

        model.InputPricePerMillionTokens = input.InputPricePerMillionTokens;
        model.OutputPricePerMillionTokens = input.OutputPricePerMillionTokens;
        model.CachedInputPricePerMillionTokens = input.CachedInputPricePerMillionTokens;
        model.IsActive = input.IsActive;
        model.UpdatedAt = now;

        await context.SaveChangesAsync(cancellationToken);

        return ToDto(model);
    }

    internal static AiModelDto ToDto(AiModel m) => new()
    {
        Id = m.Id,
        Name = m.Name,
        Identifier = m.Identifier,
        Kind = m.Kind,
        SupportsTextInput = m.SupportsTextInput,
        SupportsTextOutput = m.SupportsTextOutput,
        SupportsImageInput = m.SupportsImageInput,
        SupportsImageOutput = m.SupportsImageOutput,
        SupportsFileInput = m.SupportsFileInput,
        SupportsFileOutput = m.SupportsFileOutput,
        SupportsVoiceInput = m.SupportsVoiceInput,
        SupportsVoiceOutput = m.SupportsVoiceOutput,
        EmbeddingDimensions = m.EmbeddingDimensions,
        IsDefaultRagModel = m.IsDefaultRagModel,
        InputPricePerMillionTokens = m.InputPricePerMillionTokens,
        OutputPricePerMillionTokens = m.OutputPricePerMillionTokens,
        CachedInputPricePerMillionTokens = m.CachedInputPricePerMillionTokens,
        IsActive = m.IsActive,
        CreatedAt = m.CreatedAt,
        UpdatedAt = m.UpdatedAt
    };
}
