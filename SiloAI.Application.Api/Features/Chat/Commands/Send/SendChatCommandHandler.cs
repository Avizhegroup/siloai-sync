using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SiloAI.Agent.Chat;

namespace SiloAI.Application.Api.Features;
public class SendChatCommandHandler(
    ChatAgentService agentService,
    AiApiContext dbContext,
    IServiceScopeFactory scopeFactory,
    ILogger<SendChatCommandHandler> logger,
    IPricingEngine pricingEngine,
    ICreditLedgerService ledgerService)
    : IRequestHandler<SendChatCommand, SendChatResponse>
{
    public async Task<SendChatResponse> Handle(SendChatCommand request, CancellationToken cancellationToken)
    {
        if (!await HasCreditAsync(request.CustomerId, cancellationToken))
        {
            throw new InsufficientCreditException();
        }

        var ownerKey = ChatSessionOwnerKey.ForCustomer(request.CustomerId);

        AiChatSession? chatSession = null;

        string? existingSessionJson = null;

        if (request.ConversationId is not null)
        {
            chatSession = await dbContext.AiChatSessions
                .FirstOrDefaultAsync(s => s.Id == request.ConversationId.Value, cancellationToken);

            if (chatSession is null || chatSession.OwnerKey != ownerKey)
            { 
                throw new ConversationNotFoundException(); 
            }

            existingSessionJson = chatSession.SessionState;
        }

        await agentService.InitChatAgent(new() { request.DocType });

        var query = new CopilotMessageRequest
        {
            Text = request.Message,
            Username = request.Username,
            SiloChatId = Guid.NewGuid().ToString(),
            IsUser = true,
            Datetime = DateTime.Now
        };

        var result = await agentService.SendWithAgentSessionAsync(existingSessionJson, query);

        var priceUsage = result.PriceUsage;

        var now = DateTime.Now;

        if (chatSession is null)
        {
            chatSession = new AiChatSession
            {
                Id = Guid.NewGuid(),
                OwnerKey = ownerKey,
                ChatType = "Chat",
                CreatedAt = now
            };
            dbContext.AiChatSessions.Add(chatSession);
        }

        if (request.CustomerId.HasValue)
        {
            var customerId = request.CustomerId.Value;

            // The turn index is fixed before charging so a retried request for the same
            // turn reproduces the same idempotency key and can never be charged twice.
            var turnIndex = chatSession.TurnIndex + 1;

            var charge = await pricingEngine.CalculateAsync(
                new TokenUsageInput(
                    result.TokenUsage.InputTokenCount,
                    result.TokenUsage.CachedInputTokenCount,
                    result.TokenUsage.OutputTokenCount),
                UsageFeature.SupportChat,
                cancellationToken);

            var usageRecord = new UsageRecord
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                Feature = UsageFeature.SupportChat,
                Model = string.Empty,
                InputTokens = (int)Math.Min(int.MaxValue, result.TokenUsage.InputTokenCount),
                CachedTokens = (int)Math.Min(int.MaxValue, result.TokenUsage.CachedInputTokenCount),
                OutputTokens = (int)Math.Min(int.MaxValue, result.TokenUsage.OutputTokenCount),
                CostUsd = charge.CostUsd,
                FxRateUsed = charge.FxRateUsed,
                MultiplierUsed = charge.MultiplierUsed,
                FloorTomanUsed = charge.FloorTomanUsed,
                ChargeToman = charge.ChargeToman,
                ConversationId = chatSession.Id,
                CreatedAt = DateTime.UtcNow
            };

            var chargeOutcome = await ledgerService.ChargeAsync(
                customerId, charge, usageRecord,
                idempotencyKey: $"chat:{chatSession.Id}:{turnIndex}",
                cancellationToken);

            if (chargeOutcome == ChargeOutcome.InsufficientBalance)
                throw new InsufficientCreditException();

            if (chargeOutcome == ChargeOutcome.Success)
                chatSession.TurnIndex = turnIndex;

            // Legacy credit cache (USD) — decrement by the same Toman amount that was
            // actually charged, converted back with the same snapshot rate, so the two
            // columns stay consistent until the legacy column is removed.
            var chargeUsd = charge.FxRateUsed > 0
                ? Math.Round(charge.ChargeToman / charge.FxRateUsed, 8)
                : 0m;

            await dbContext.Customers
                .Where(c => c.Id == customerId)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(c => c.RemainingCredit,
                        c => Math.Max(0, c.RemainingCredit - chargeUsd)),
                    cancellationToken);
        }

        var updatedSessionJson = result.SerializedSession;

        chatSession.UpdatedAt = now;

        await dbContext.SaveChangesAsync(cancellationToken);

        var instructionKey = request.DocType;
        var userAsk = request.Message;
        var botAnswer = result.Response;
        var conversationCustomerId = request.CustomerId;

        _ = Task.Run(async () =>
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AiApiContext>();
            try
            {
                db.AiConversations.Add(new AiConversation
                {
                    UserAsk = userAsk,
                    BotAnswer = botAnswer.ResponseText,
                    InstructionKey = (int)instructionKey,
                    CreditUsage = null,
                    LocalConversationId = 0,
                    CustomerId = conversationCustomerId ?? 0,
                    CreatedAt = DateTime.Now
                });
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to save AI conversation to database.");
            }
        });
        
        return new SendChatResponse
        {
            ResponseText = result.Response.ResponseText,
            ConversationId = chatSession.Id,
            TokenUsage = result.TokenUsage,
            PriceUsage = priceUsage
        };
    }

    private async Task<bool> HasCreditAsync(int? customerId, CancellationToken cancellationToken)
    {
        if (customerId is null)
        {
            return true; 
        }

        // The ledger is the single source of truth for balances; the legacy
        // Customer.RemainingCredit column is only a display cache and must not
        // gate real (paid) AI calls.
        var balance = await ledgerService.GetBalanceAsync(customerId.Value, cancellationToken);

        return balance > 0;
    }
}
