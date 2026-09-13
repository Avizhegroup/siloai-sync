using SiloAI.Agent.Chat;

namespace SiloAI.Application.Api.Features;

public class DeleteRagInstructionCommandHandler(AiApiContext context, ChatAgentCache agentCache) : IRequestHandler<DeleteRagInstructionCommand, bool>
{
    public async Task<bool> Handle(DeleteRagInstructionCommand request, CancellationToken cancellationToken)
    {
        var instruction = await context.RagInstructions
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (instruction is null)
            return false;

        context.RagInstructions.Remove(instruction);
        await context.SaveChangesAsync(cancellationToken);

        agentCache.InvalidateInstructions(instruction.DocType);

        return true;
    }
}
