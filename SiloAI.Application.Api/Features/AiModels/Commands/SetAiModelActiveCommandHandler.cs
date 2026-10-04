namespace SiloAI.Application.Api.Features;

public class SetAiModelActiveCommandHandler(AiApiContext context) : IRequestHandler<SetAiModelActiveCommand, AiModelDto>
{
    public async Task<AiModelDto> Handle(SetAiModelActiveCommand request, CancellationToken cancellationToken)
    {
        var model = await context.AiModels.FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken)
            ?? throw new InvalidOperationException($"AiModel '{request.Id}' was not found.");

        if (!request.IsActive && model.IsDefaultRagModel)
            throw new InvalidOperationException(
                "Cannot deactivate the active RAG (embedding) model — assign a different model as " +
                "the default RAG model first.");

        if (!request.IsActive)
        {
            var stillAssigned = await context.CustomerModelAssignments
                .AsNoTracking()
                .AnyAsync(a => a.AiModelId == model.Id, cancellationToken);

            if (stillAssigned)
                throw new InvalidOperationException(
                    "Cannot deactivate a model that is still assigned to a customer or set as a " +
                    "feature's default — reassign those first.");
        }

        model.IsActive = request.IsActive;
        model.UpdatedAt = DateTime.Now;

        await context.SaveChangesAsync(cancellationToken);

        return UpsertAiModelCommandHandler.ToDto(model);
    }
}
