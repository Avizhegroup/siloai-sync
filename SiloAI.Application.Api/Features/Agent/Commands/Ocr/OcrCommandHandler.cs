using SiloAI.Agent.Chat;
using SiloAI.Application.Shared.Contracts.AiModels;

namespace SiloAI.Application.Api.Features;

public class OcrCommandHandler(
    ChatAgentService agentService,
    AiApiContext dbContext,
    IPricingEngine pricingEngine,
    ICreditLedgerService ledgerService,
    IAiModelResolver modelResolver) : IRequestHandler<OcrCommand, OcrResponse>
{
    public async Task<OcrResponse> Handle(OcrCommand request, CancellationToken cancellationToken)
    {
        if (!await HasCreditAsync(request.CustomerId, cancellationToken))
            throw new InsufficientCreditException();

        var resolvedModel = await modelResolver.ResolveAsync(request.CustomerId, UsageFeature.Ocr, cancellationToken);

        await agentService.InitChatAgent(modelName: resolvedModel.Identifier);

        // NOTE (still open, not part of this change): SendImageAndGetTextAsync only returns the
        // extracted text, not token usage, so the charge below is always priced as (0, 0, 0)
        // input/output/cached — i.e. always the feature's floor charge, never the real usage
        // cost. And the idempotency key is a fresh GUID generated on every call, so a client
        // retry after a timeout is NOT protected against double-charging the way ragchat/chat
        // turns are. Both were flagged previously and are unrelated to AI model management.
        var extractedText = await agentService.SendImageAndGetTextAsync(request.ImageData
            , request.MediaType
            , request.DocType);

        if (request.CustomerId.HasValue)
        {
            var customerId = request.CustomerId.Value;

            var charge = await pricingEngine.CalculateAsync(
                new TokenUsageInput(0, 0, 0),
                UsageFeature.Ocr,
                resolvedModel.AiModelId,
                cancellationToken);

            var usageRecord = new UsageRecord
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                Feature = UsageFeature.Ocr,
                Model = resolvedModel.Identifier,
                InputTokens = 0,
                CachedTokens = 0,
                OutputTokens = 0,
                CostUsd = charge.CostUsd,
                FxRateUsed = charge.FxRateUsed,
                MultiplierUsed = charge.MultiplierUsed,
                FloorTomanUsed = charge.FloorTomanUsed,
                ChargeToman = charge.ChargeToman,
                ConversationId = null,
                CreatedAt = DateTime.UtcNow
            };

            var chargeOutcome = await ledgerService.ChargeAsync(
                customerId, charge, usageRecord,
                idempotencyKey: $"ocr:{Guid.NewGuid():N}",
                cancellationToken);

            if (chargeOutcome == ChargeOutcome.InsufficientBalance)
                throw new InsufficientCreditException();
        }

        return new OcrResponse { ExtractedText = extractedText };
    }

    private async Task<bool> HasCreditAsync(int? customerId, CancellationToken cancellationToken)
    {
        if (customerId is null) return true;

        var balance = await ledgerService.GetBalanceAsync(customerId.Value, cancellationToken);

        return balance > 0;
    }
}
