using SiloAI.Agent.Chat;
using System.Text;

namespace SiloAI.Application.Api.Features;

public class RagChatSendHandler(
    ChatAgentService agentService,
    IRagSearchService search,
    IMediator mediator,
    AiApiContext dbContext,
    ChatAgentCache agentCache,
    IPricingEngine pricingEngine,
    ICreditLedgerService ledgerService) : IRequestHandler<RagChatSendCommand, RagChatResponse>
{
    public async Task<RagChatResponse> Handle(RagChatSendCommand request, CancellationToken cancellationToken)
    {
        if (!await HasCreditAsync(request.CustomerId, cancellationToken))
        {
            throw new InsufficientCreditException();
        }

        var ownerKey = ChatSessionOwnerKey.ForOwnerId(request.OwnerId);

        AiChatSession? chatSession = null;
        string? existingSessionJson = null;

        if (request.ConversationId.HasValue)
        {
            chatSession = await dbContext.AiChatSessions
                .FirstOrDefaultAsync(s => s.Id == request.ConversationId.Value, cancellationToken);

            if (chatSession is null || chatSession.OwnerKey != ownerKey)
                throw new ConversationNotFoundException();

            existingSessionJson = chatSession.SessionState;
        }


        var instructions = await agentCache.GetOrCreateInstructionsAsync((int)request.DocType, async () =>
            (IReadOnlyList<CachedRagInstruction>)await dbContext.RagInstructions
                .Where(p => p.DocType == (int)request.DocType && p.IsActive)
                .AsNoTracking()
                .Select(p => new CachedRagInstruction(p.Content, p.IsSystematic, p.CreateDateTime))
                .ToListAsync(cancellationToken));

        var agentInstructions = BuildAgentInstructions(instructions);

        // This handler already performs its own, DocType/Key-filtered retrieval below and
        // augments the message itself, so the agent's built-in auto-RAG context provider is
        // disabled here to avoid a second, unfiltered retrieval pass (extra embedding call,
        // extra DB round-trips, and duplicate chunk content being sent to the model).
        agentService.InitChatAgentWithInstructions(
            agentInstructions, request.RagModel, includeAutoRagContext: false);

        var topK = request.TopK <= 0 ? 5 : Math.Clamp(request.TopK, 1, 20);
     
        var hits = await search.SearchAsync(
            request.Message, topK, request.DocType, request.Key, cancellationToken);

        // Materialized above — no extra DB round-trip here.
        var systematicInstructions = instructions.FirstOrDefault(p => p.IsSystematic);

        if (systematicInstructions is null)
        {
            throw new ConversationNotFoundException();
        }

        var augmentedMessage = BuildAugmentedMessage(
            request.Message, hits, request.IsMainChat, systematicInstructions.Content);

        var query = new CopilotMessageRequest
        {
            Text = augmentedMessage,
            Username = request.Username,
            SiloChatId = Guid.NewGuid().ToString(),
            IsUser = true,
            Datetime = DateTime.Now
        };

        var result = await agentService.SendWithAgentSessionAsync(existingSessionJson, query);

        var now = DateTime.UtcNow;

        if (chatSession is null)
        {
            chatSession = new AiChatSession
            {
                Id = Guid.NewGuid(),
                OwnerKey = ownerKey,
                ChatType = "Rag",
                CreatedAt = now
            };
            dbContext.AiChatSessions.Add(chatSession);
        }

        chatSession.SessionState = result.SerializedSession;
        chatSession.UpdatedAt = now;

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
                Model = request.RagModel ?? string.Empty,
                InputTokens = (int)Math.Min(int.MaxValue, result.TokenUsage.InputTokenCount),
                CachedTokens = (int)Math.Min(int.MaxValue, result.TokenUsage.CachedInputTokenCount),
                OutputTokens = (int)Math.Min(int.MaxValue, result.TokenUsage.OutputTokenCount),
                CostUsd = charge.CostUsd,
                FxRateUsed = charge.FxRateUsed,
                MultiplierUsed = charge.MultiplierUsed,
                FloorTomanUsed = charge.FloorTomanUsed,
                ChargeToman = charge.ChargeToman,
                ConversationId = chatSession.Id,
                CreatedAt = now
            };

            var chargeOutcome = await ledgerService.ChargeAsync(
                customerId, charge, usageRecord,
                idempotencyKey: $"ragchat:{chatSession.Id}:{turnIndex}",
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

        await dbContext.SaveChangesAsync(cancellationToken);

        var citations = request.IsMainChat
            ? new List<RagChatCitationDto>()
            : hits.Select(h => new RagChatCitationDto
            {
                ChunkId = h.ChunkId,
                DocumentId = h.DocumentId,
                FileName = h.FileName,
                Category = h.Category,
                ChunkIndex = h.ChunkIndex,
                Similarity = h.Similarity,
                Snippet = Truncate(h.Content, 280)
            }).ToList();

        return new RagChatResponse
        {
            ResponseText = result.Response.ResponseText,
            ConversationId = chatSession.Id,
            TokenUsage = result.TokenUsage,
            Citations = citations,
            PriceUsage = result.PriceUsage
        };
    }

    internal static string BuildAgentInstructions(IReadOnlyList<CachedRagInstruction> instructions)
    {
        var docTypeInstructionsText = string.Join("\n---\n", instructions
            .OrderBy(i => i.CreateDateTime)
            .Select(i => i.Content));

        return $"{docTypeInstructionsText}";
    }

    private static string BuildAugmentedMessage(
        string userQuestion,
        IReadOnlyList<RagSearchHit> hits,
        bool isMainChat,
        string augmentedMessageTemplate)
    {
        string chunksText;

        if (hits is null || hits.Count == 0)
        {
            chunksText = "(هیچ قطعه‌ای از پایگاه دانش برای این پرسش پیدا نشد.)";
        }
        else
        {
            var sb = new StringBuilder();
            for (var i = 0; i < hits.Count; i++)
            {
                var h = hits[i];
                if (!isMainChat)
                    sb.AppendLine($"[{i + 1}] فایل: {h.FileName} | قطعه: {h.ChunkIndex} | شباهت: {h.Similarity:F3}");
                sb.AppendLine(h.Content);
                sb.AppendLine("---");
            }
            chunksText = sb.ToString().TrimEnd();
        }

        return augmentedMessageTemplate
            .Replace("{DOCTYPE_INSTRUCTIONS}", string.Empty)
            .Replace("{CHUNKS}", chunksText)
            .Replace("{QUESTION}", userQuestion);
    }

    private static string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        return value.Length <= maxLength ? value : value[..maxLength] + "…";
    }

    private async Task<bool> HasCreditAsync(
    int? customerId,
    CancellationToken cancellationToken)
    {
        if (customerId is null)
            return true;

        // The ledger is the single source of truth for balances; the legacy
        // Customer.RemainingCredit column is only a display cache and must not
        // gate real (paid) AI calls.
        var balance = await ledgerService.GetBalanceAsync(customerId.Value, cancellationToken);

        return balance > 0;
    }
}
