using SiloAI.Agent.Chat;

namespace SiloAI.Application.Api.Features;

public class RagChatNewSessionCommandHandler(
    ChatAgentService agentService,
    AiApiContext dbContext,
    ChatAgentCache agentCache) : IRequestHandler<RagChatNewSessionCommand, RagChatResponse>
{
    public async Task<RagChatResponse> Handle(RagChatNewSessionCommand request, CancellationToken cancellationToken)
    {
        var instructions = await agentCache.GetOrCreateInstructionsAsync((int)request.DocType, async () =>
            (IReadOnlyList<CachedRagInstruction>)await dbContext.RagInstructions
                .Where(p => p.DocType == (int)request.DocType && p.IsActive)
                .AsNoTracking()
                .Select(p => new CachedRagInstruction(p.Content, p.IsSystematic, p.CreateDateTime))
                .ToListAsync(cancellationToken));

        var agentInstructions = RagChatSendHandler.BuildAgentInstructions(instructions);

        agentService.InitChatAgentWithInstructions(
            agentInstructions, request.RagModel, includeAutoRagContext: false);

        var session = await agentService.CreateNewSessionAsync();

        var sessionJson = await agentService.SerializeSessionAsync(session);

        var now = DateTime.Now;
        var chatSession = new AiChatSession
        {
            Id = Guid.NewGuid(),
            OwnerKey = ChatSessionOwnerKey.ForOwnerId(request.OwnerId),
            ChatType = "Rag",
            SessionState = sessionJson,
            CreatedAt = now,
            UpdatedAt = now
        };

        dbContext.AiChatSessions.Add(chatSession);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new RagChatResponse
        {
            ResponseText = string.Empty,
            ConversationId = chatSession.Id,
            Citations = new()
        };
    }
}
