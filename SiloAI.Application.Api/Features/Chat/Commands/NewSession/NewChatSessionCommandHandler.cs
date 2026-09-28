using SiloAI.Agent.Chat;
using SiloAI.Application.Shared.Contracts.AiModels;
using NewChatSessionCommand = SiloAI.Application.Shared.Features.NewChatSessionCommand;

namespace SiloAI.Application.Api.Features;

public class NewChatSessionCommandHandler(
    ChatAgentService agentService,
    AiApiContext dbContext,
    ICreditLedgerService ledgerService,
    IAiModelResolver modelResolver) : IRequestHandler<NewChatSessionCommand, NewSessionResponse>
{
    public async Task<NewSessionResponse> Handle(NewChatSessionCommand request, CancellationToken cancellationToken)
    {
        if (!await HasCreditAsync(request.CustomerId, cancellationToken))
            throw new InsufficientCreditException();

        var resolvedModel = await modelResolver.ResolveAsync(request.CustomerId, UsageFeature.SupportChat, cancellationToken);

        await agentService.InitChatAgent(new() { request.DocType }, modelName: resolvedModel.Identifier);

        var session = await agentService.CreateNewSessionAsync();
        var sessionJson = await agentService.SerializeSessionAsync(session);

        var now = DateTime.Now;
        var chatSession = new AiChatSession
        {
            Id = Guid.NewGuid(),
            OwnerKey = ChatSessionOwnerKey.ForCustomer(request.CustomerId),
            ChatType = "Chat",
            SessionState = sessionJson,
            CreatedAt = now,
            UpdatedAt = now
        };

        dbContext.AiChatSessions.Add(chatSession);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new NewSessionResponse { ConversationId = chatSession.Id };
    }

    private async Task<bool> HasCreditAsync(int? customerId, CancellationToken cancellationToken)
    {
        if (customerId is null) return true;

        // The ledger is the single source of truth for balances; the legacy
        // Customer.RemainingCredit column is only a display cache and must not gate this.
        var balance = await ledgerService.GetBalanceAsync(customerId.Value, cancellationToken);

        return balance > 0;
    }
}
