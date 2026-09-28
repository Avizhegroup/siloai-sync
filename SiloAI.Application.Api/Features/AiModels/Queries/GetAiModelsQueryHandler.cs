namespace SiloAI.Application.Api.Features;

public class GetAiModelsQueryHandler(AiApiContext context) : IRequestHandler<GetAiModelsQuery, List<AiModelDto>>
{
    public async Task<List<AiModelDto>> Handle(GetAiModelsQuery request, CancellationToken cancellationToken)
    {
        return await context.AiModels
            .AsNoTracking()
            .OrderBy(m => m.Kind).ThenBy(m => m.Name)
            .Select(m => new AiModelDto
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
            })
            .ToListAsync(cancellationToken);
    }
}
