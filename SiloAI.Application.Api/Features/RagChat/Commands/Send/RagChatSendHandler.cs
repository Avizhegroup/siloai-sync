using SiloAI.Agent.Chat;
using SiloAI.Application.Shared.Contracts.AiModels;
using System.Text;

namespace SiloAI.Application.Api.Features;

public class RagChatSendHandler(
    ChatAgentService agentService,
    IRagSearchService search,
    IMediator mediator,
    AiApiContext dbContext,
    ChatAgentCache agentCache,
    IPricingEngine pricingEngine,
    ICreditLedgerService ledgerService,
    IAiModelResolver modelResolver) : IRequestHandler<RagChatSendCommand, RagChatResponse>
{
    /// <summary>AiChatSession.ChatType of a conversation that uses the knowledge base (RAG).</summary>
    internal const string RagChatType = "Rag";

    /// <summary>
    /// AiChatSession.ChatType of a conversation that runs on instructions alone — no retrieval,
    /// no search tool. Same value the legacy plain-chat handlers already use for "no RAG".
    /// </summary>
    internal const string PlainChatType = "Chat";

    public async Task<RagChatResponse> Handle(RagChatSendCommand request, CancellationToken cancellationToken)
    {
        //if (!await HasCreditAsync(request.CustomerId, cancellationToken))
        //{
        //    throw new InsufficientCreditException();
        //}

        var ownerKey = ChatSessionOwnerKey.ForOwnerId(request.OwnerId);

        AiChatSession? chatSession = null;
        string? existingSessionJson = null;

        if (request.ConversationId.HasValue)
        {
            chatSession = await dbContext.AiChatSessions
                .FirstOrDefaultAsync(s => s.Id == request.ConversationId.Value, cancellationToken);

            //if (chatSession is null || chatSession.OwnerKey != ownerKey)
            //    throw new ConversationNotFoundException();

            existingSessionJson = chatSession.SessionState;
        }

        // A conversation's mode is fixed when it is created: an existing conversation keeps the
        // mode it started with (read from its stored ChatType), and request.UseRag only decides
        // for a brand-new one. Reusing one AgentSession across agents configured differently
        // (with vs. without the search tool, whose calls end up in the session history) is not a
        // reliable combination, so a client can't flip a live conversation between modes — and
        // doesn't have to repeat UseRag on every Send.
        var useRag = chatSession is not null
            ? !string.Equals(chatSession.ChatType, PlainChatType, StringComparison.Ordinal)
            : request.UseRag;

        var instructions = await agentCache.GetOrCreateInstructionsAsync((int)request.DocType, async () =>
            (IReadOnlyList<CachedRagInstruction>)await dbContext.RagInstructions
                .Where(p => p.DocType == (int)request.DocType && p.IsActive)
                .AsNoTracking()
                .Select(p => new CachedRagInstruction(p.Content, p.IsSystematic, p.CreateDateTime))
                .ToListAsync(cancellationToken));

        var agentInstructions = BuildAgentInstructions(instructions, includeSystematic: useRag);

        // Resolved once up front: needed both to run the chat call itself and (below) to price
        // it correctly — a customer's own model assignment for SupportChat if one exists,
        // otherwise the global default for that feature.
        var resolvedModel = await modelResolver.ResolveAsync(request.CustomerId, UsageFeature.SupportChat, cancellationToken);

        // This handler already performs its own, DocType/Key-filtered retrieval below and
        // augments the message itself, so the agent's built-in auto-RAG context provider is
        // disabled here to avoid a second, unfiltered retrieval pass (extra embedding call,
        // extra DB round-trips, and duplicate chunk content being sent to the model).
        // includeAutoRagContext stays false: this handler already performs its own,
        // DocType/Key-filtered retrieval below and augments the message itself. includeSearchTool
        // is new and separate — it gives the agent a callable search tool, scoped to the same
        // DocType/Key, so it can search again with a refined query if the chunks already
        // augmented into the message don't answer the question (e.g. the conversation has moved
        // on since the turn where retrieval last ran).
        //
        // Without RAG (useRag == false) the agent gets no search tool and no DocType/Key scope
        // (there is nothing to scope), so it runs on the instructions alone.
        agentService.InitChatAgentWithInstructions(
            agentInstructions, resolvedModel, includeAutoRagContext: false,
            ragDocType: useRag ? request.DocType : null,
            ragKey: useRag ? request.Key : null,
            includeSearchTool: useRag);

        IReadOnlyList<RagSearchHit> hits = [];
        string messageText;

        if (useRag)
        {
            var topK = 20;

            hits = await search.SearchAsync(
                request.Message, topK, request.DocType, request.Key, cancellationToken);

            // Materialized above — no extra DB round-trip here.
            var systematicInstructions = instructions.FirstOrDefault(p => p.IsSystematic);

            if (systematicInstructions is null)
            {
                throw new ConversationNotFoundException();
            }

            messageText = BuildAugmentedMessage(
                request.Message, hits, request.IsMainChat, systematicInstructions.Content);
        }
        else
        {
            // Instructions-only chat: the user's message goes to the model as written. There is
            // no retrieval to splice in, so the systematic message template (which exists to
            // carry {CHUNKS}/{QUESTION}) isn't needed — and isn't required to exist.
            messageText = request.Message;
        }

        var query = new CopilotMessageRequest
        {
            Text = messageText,
            Username = request.Username,
            SiloChatId = Guid.NewGuid().ToString(),
            IsUser = true,
            Datetime = DateTime.Now
        };

        var result = await agentService.SendWithAgentSessionAsync(existingSessionJson, query);

        var now = DateTime.Now;

        if (chatSession is null)
        {
            chatSession = new AiChatSession
            {
                Id = Guid.NewGuid(),
                OwnerKey = ownerKey,
                ChatType = useRag ? RagChatType : PlainChatType,
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
                resolvedModel.AiModelId,
                cancellationToken);

            var usageRecord = new UsageRecord
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                Feature = UsageFeature.SupportChat,
                Model = resolvedModel.Identifier,
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

            //if (chargeOutcome == ChargeOutcome.InsufficientBalance)
            //    throw new InsufficientCreditException();

            //if (chargeOutcome == ChargeOutcome.Success)
            //    chatSession.TurnIndex = turnIndex;

            // Legacy credit cache (USD) — decrement by the same Toman amount that was
            // actually charged, converted back with the same snapshot rate, so the two
            // columns stay consistent until the legacy column is removed.
            var chargeUsd = charge.FxRateUsed > 0
                ? Math.Round(charge.ChargeToman / charge.FxRateUsed, 8)
                : 0m;

            //await dbContext.Customers
            //    .Where(c => c.Id == customerId)
            //    .ExecuteUpdateAsync(s => s
            //        .SetProperty(c => c.RemainingCredit,
            //            c => Math.Max(0, c.RemainingCredit - chargeUsd)),
            //        cancellationToken);
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

    /// <param name="includeSystematic">
    /// true (default, RAG chat): every active instruction is included, exactly as before.
    /// false (instructions-only chat): instructions flagged IsSystematic are left out — those are
    /// the RAG message template (with {CHUNKS}/{QUESTION} placeholders), which would only put
    /// literal placeholder text into the system prompt when there is no retrieval to fill it.
    /// </param>
    internal static string BuildAgentInstructions(
        IReadOnlyList<CachedRagInstruction> instructions, bool includeSystematic = true)
    {
        var docTypeInstructionsText = string.Join("\n---\n", instructions
            .Where(i => includeSystematic || !i.IsSystematic)
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
