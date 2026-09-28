namespace SiloAI.Application.Shared.Features;

public class GetAiModelsQuery : IRequest<List<AiModelDto>>
{
}

public class UpsertAiModelCommand : IRequest<AiModelDto>
{
    public UpsertAiModelRequest Model { get; set; }
}

public class SetAiModelActiveCommand : IRequest<AiModelDto>
{
    public Guid Id { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>Marks a Rag-kind model as THE active embedding model, clearing the flag on any other.</summary>
public class SetDefaultRagModelCommand : IRequest<AiModelDto>
{
    public Guid AiModelId { get; set; }
}
