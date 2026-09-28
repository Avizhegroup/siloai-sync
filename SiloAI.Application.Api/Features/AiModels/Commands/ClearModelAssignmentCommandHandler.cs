namespace SiloAI.Application.Api.Features;

public class ClearModelAssignmentCommandHandler(AiApiContext context) : IRequestHandler<ClearModelAssignmentCommand, bool>
{
    public async Task<bool> Handle(ClearModelAssignmentCommand request, CancellationToken cancellationToken)
    {
        var existing = await context.CustomerModelAssignments
            .FirstOrDefaultAsync(a => a.CustomerId == request.CustomerId && a.Feature == request.Feature, cancellationToken);

        if (existing is null)
            return false;

        context.CustomerModelAssignments.Remove(existing);
        await context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
