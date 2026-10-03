namespace SiloAI.Application.Api.Features;

public class SetModelAssignmentCommandHandler(AiApiContext context) : IRequestHandler<SetModelAssignmentCommand, ModelAssignmentDto>
{
    public async Task<ModelAssignmentDto> Handle(SetModelAssignmentCommand request, CancellationToken cancellationToken)
    {
        var input = request.Request;

        var model = await context.AiModels
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == input.AiModelId, cancellationToken)
            ?? throw new InvalidOperationException($"AiModel '{input.AiModelId}' was not found.");

        if (model.Kind != AiModelKind.Normal)
            throw new InvalidOperationException(
                "Only a Normal-kind model can be assigned to a feature. The RAG/embedding model is " +
                "shared infrastructure, set separately as the default RAG model.");

        if (!model.IsActive)
            throw new InvalidOperationException("Cannot assign an inactive model.");

        var existing = await context.CustomerModelAssignments
            .FirstOrDefaultAsync(a => a.CustomerId == input.CustomerId && a.Feature == input.Feature, cancellationToken);

        var now = DateTime.Now;

        if (existing is null)
        {
            existing = new CustomerModelAssignment
            {
                Id = Guid.NewGuid(),
                CustomerId = input.CustomerId,
                Feature = input.Feature,
                CreatedAt = now
            };
            context.CustomerModelAssignments.Add(existing);
        }

        existing.AiModelId = input.AiModelId;
        existing.UpdatedAt = now;

        await context.SaveChangesAsync(cancellationToken);

        return new ModelAssignmentDto
        {
            Feature = input.Feature,
            AiModelId = model.Id,
            AiModelName = model.Name,
            IsOverride = input.CustomerId is not null
        };
    }
}
