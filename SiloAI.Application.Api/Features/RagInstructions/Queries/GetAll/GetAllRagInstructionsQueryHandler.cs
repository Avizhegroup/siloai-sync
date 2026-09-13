namespace SiloAI.Application.Api.Features;

public class GetAllRagInstructionsQueryHandler(AiApiContext context) : IRequestHandler<GetAllRagInstructionsQuery, List<RagInstructionDto>>
{
    public async Task<List<RagInstructionDto>> Handle(GetAllRagInstructionsQuery request, CancellationToken cancellationToken)
    {
        var query = context.RagInstructions.AsNoTracking().AsQueryable();

        if (request.DocType.HasValue)
        {
            query = query.Where(x => x.DocType == (int)request.DocType);
        }

        if (request.IsActive.HasValue)
        { 
            query = query.Where(x => x.IsActive == request.IsActive.Value);
        }

        return await query.OrderBy(x => x.CreateDateTime)
                          .Select(x => MapInstruction(x))
                          .ToListAsync(cancellationToken);
    }

    private static RagInstructionDto MapInstruction(RagInstruction x) => new()
    {
        Id = x.Id,
        DocType = (RagDocType)x.DocType,
        Key = x.Key,
        Category = x.Category,
        Tags = x.Tags,
        Content = x.Content,
        IsSystematic = x.IsSystematic,
        IsActive = x.IsActive,
        CreateDateTime = x.CreateDateTime,
        LastUpdateDateTime = x.LastUpdateDateTime
    };
}
