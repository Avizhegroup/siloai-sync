using SiloAI.Application.Api.Services;
using SiloAI.Agent.Tasks;
using SiloAI.Application.Shared.Contracts.AiModels;

namespace SiloAI.Application.Api.Features;

public class OcrCommandHandler(
    AgentFileTaskService agentFileTaskService,
    AiApiContext dbContext,
    IPricingEngine pricingEngine,
    ICreditLedgerService ledgerService,
    IAiModelResolver modelResolver) : IRequestHandler<OcrCommand, OcrResponse>
{
    public async Task<OcrResponse> Handle(OcrCommand request, CancellationToken cancellationToken)
    {
        // Refuse before the provider call when the customer has no credit.
        await ledgerService.EnsureCreditAsync(request.CustomerId, cancellationToken);

        var resolvedModel = await modelResolver.ResolveAsync(request.CustomerId, UsageFeature.Ocr, cancellationToken);

        var extraction = await agentFileTaskService.ExtractTextWithUsageAsync(
            request.ImageData, request.MediaType, resolvedModel, request.DocType, request.Key, cancellationToken);

        if (request.CustomerId.HasValue)
        {
            var customerId = request.CustomerId.Value;

            var charge = await pricingEngine.CalculateAsync(
                new TokenUsageInput(extraction.InputTokens, extraction.CachedInputTokens, extraction.OutputTokens),
                UsageFeature.Ocr,
                resolvedModel.AiModelId,
                cancellationToken);

            var usageRecord = new UsageRecord
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                Feature = UsageFeature.Ocr,
                Model = resolvedModel.Identifier,
                InputTokens = CreditGuard.ToInt(extraction.InputTokens),
                CachedTokens = CreditGuard.ToInt(extraction.CachedInputTokens),
                OutputTokens = CreditGuard.ToInt(extraction.OutputTokens),
                CostUsd = charge.CostUsd,
                FxRateUsed = charge.FxRateUsed,
                MultiplierUsed = charge.MultiplierUsed,
                FloorTomanUsed = charge.FloorTomanUsed,
                ChargeToman = charge.ChargeToman,
                ConversationId = null,
                CreatedAt = DateTime.Now
            };

            // A client-supplied RequestId makes a retried call idempotent; without one every call is a
            // distinct charge (same contract as image generation / analysis).
            var outcome = await ledgerService.ChargeAsync(
                customerId, charge, usageRecord,
                idempotencyKey: $"ocr:{(request.RequestId ?? Guid.NewGuid()):N}",
                cancellationToken);

            if (outcome == ChargeOutcome.InsufficientBalance)
                throw new InsufficientCreditException();
        }

        return new OcrResponse { ExtractedText = extraction.Text };
    }
}
