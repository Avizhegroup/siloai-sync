namespace SiloAI.Application.Api.Features;

public class UpsertAiModelCommandHandler(AiApiContext context) : IRequestHandler<UpsertAiModelCommand, AiModelDto>
{
    public async Task<AiModelDto> Handle(UpsertAiModelCommand request, CancellationToken cancellationToken)
    {
        var input = request.Model;

        if (input.Kind == AiModelKind.Rag && (input.EmbeddingDimensions is null || input.EmbeddingDimensions <= 0))
            throw new InvalidOperationException("A RAG (embedding) model requires a positive EmbeddingDimensions value.");

        if (input.InputPricePerMillionTokens < 0 || input.OutputPricePerMillionTokens < 0
            || input.CachedInputPricePerMillionTokens < 0 || input.PricePerImageUsd < 0
            || input.PricePerAudioMinuteUsd < 0 || input.PricePerMillionCharactersUsd < 0)
            throw new InvalidOperationException("Prices cannot be negative.");

        string? voices = null;
        if (input.Kind == AiModelKind.TextToSpeech)
        {
            var voiceError = AiModelVoices.Validate(input.Voices);
            if (voiceError is not null)
                throw new InvalidOperationException(voiceError);

            voices = AiModelVoices.Normalize(input.Voices)
                ?? throw new InvalidOperationException("A text-to-speech model requires at least one voice.");
        }

        var duplicateIdentifier = await context.AiModels
            .AsNoTracking()
            .AnyAsync(m => m.Identifier == input.Identifier && m.Id != input.Id, cancellationToken);

        if (duplicateIdentifier)
            throw new InvalidOperationException($"A model with identifier '{input.Identifier}' already exists.");

        var now = DateTime.Now;
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
            model.EmbeddingDimensions = input.Kind == AiModelKind.Rag ? input.EmbeddingDimensions : null;
            // IsDefaultRagModel is intentionally left untouched here — toggled only via
            // SetDefaultRagModelCommand, which also enforces the single-default invariant.
        }

        // Voices are a text-to-speech-only property; every other kind stores null.
        model.Voices = voices;
        model.InputPricePerMillionTokens = input.InputPricePerMillionTokens;
        model.OutputPricePerMillionTokens = input.OutputPricePerMillionTokens;
        model.CachedInputPricePerMillionTokens = input.CachedInputPricePerMillionTokens;
        model.PricePerImageUsd = input.PricePerImageUsd;
        model.PricePerAudioMinuteUsd = input.PricePerAudioMinuteUsd;
        model.PricePerMillionCharactersUsd = input.PricePerMillionCharactersUsd;
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
        Voices = m.Voices,
        InputPricePerMillionTokens = m.InputPricePerMillionTokens,
        OutputPricePerMillionTokens = m.OutputPricePerMillionTokens,
        CachedInputPricePerMillionTokens = m.CachedInputPricePerMillionTokens,
        PricePerImageUsd = m.PricePerImageUsd,
        PricePerAudioMinuteUsd = m.PricePerAudioMinuteUsd,
        PricePerMillionCharactersUsd = m.PricePerMillionCharactersUsd,
        IsActive = m.IsActive,
        CreatedAt = m.CreatedAt,
        UpdatedAt = m.UpdatedAt
    };
}
