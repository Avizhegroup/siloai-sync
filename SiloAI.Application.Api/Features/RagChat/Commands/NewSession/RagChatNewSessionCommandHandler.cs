using SiloAI.Agent.Chat;
using SiloAI.Application.Shared.Contracts.AiModels;

namespace SiloAI.Application.Api.Features;

public class RagChatNewSessionCommandHandler(
    ChatAgentService agentService,
    AiApiContext dbContext,
    ChatAgentCache agentCache,
    IAiModelResolver modelResolver) : IRequestHandler<RagChatNewSessionCommand, RagChatResponse>
{
    public async Task<RagChatResponse> Handle(RagChatNewSessionCommand request, CancellationToken cancellationToken)
    {
        var instructions = await agentCache.GetOrCreateInstructionsAsync((int)request.DocType, async () =>
            (IReadOnlyList<CachedRagInstruction>)await dbContext.RagInstructions
                .Where(p => p.DocType == (int)request.DocType && p.IsActive)
                .AsNoTracking()
                .Select(p => new CachedRagInstruction(p.Content, p.IsSystematic, p.CreateDateTime))
                .ToListAsync(cancellationToken));

        var agentInstructions = RagChatSendHandler.BuildAgentInstructions(
            instructions, includeSystematic: request.UseRag);

        var resolvedModel = await modelResolver.ResolveAsync(request.CustomerId, UsageFeature.SupportChat, cancellationToken);

        // Mirrors RagChatSendHandler's call exactly (same includeAutoRagContext: false,
        // includeSearchTool: true, and DocType scope — RagChatNewSessionCommand has no Key
        // field, so ragKey is always null here, matching the scope RagChatSendHandler uses when
        // its own request.Key is also unset). Keeping these two call sites identical matters:
        // InitChatAgentWithInstructions resolves (and may cache) a specific AIAgent instance per
        // call, and the same AgentSession created here is later reused by Send — an agent built
        // with different tool/context configuration than the one that created a session is not a
        // reliable combination (see the ChatAgentCache docs). The same holds for UseRag: without
        // RAG there is no search tool and no DocType scope, exactly as in RagChatSendHandler, and
        // the mode is recorded in the session's ChatType so Send keeps using it.
        agentService.InitChatAgentWithInstructions(
            agentInstructions, resolvedModel, includeAutoRagContext: false,
            ragDocType: request.UseRag ? request.DocType : null,
            ragKey: null,
            includeSearchTool: request.UseRag);

        var session = await agentService.CreateNewSessionAsync();

        var sessionJson = await agentService.SerializeSessionAsync(session);

        var now = DateTime.Now;
        var chatSession = new AiChatSession
        {
            Id = Guid.NewGuid(),
            OwnerKey = ChatSessionOwnerKey.ForOwnerId(request.OwnerId),
            ChatType = request.UseRag ? RagChatSendHandler.RagChatType : RagChatSendHandler.PlainChatType,
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
