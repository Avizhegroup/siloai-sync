using SiloAI.Application.Api.Services;
using SiloAI.Agent.ImageGeneration;

namespace SiloAI.Application.Api.Features;

public class ImageGenerationCommandHandler(
    IImageGenerationAgentService agentService,
    IPricingEngine pricingEngine,
    ICreditLedgerService ledgerService,
    IAiModelResolver modelResolver) : IRequestHandler<ImageGenerationCommand, ImageGenerationResponse>
{
    public async Task<ImageGenerationResponse> Handle(ImageGenerationCommand request, CancellationToken cancellationToken)
    {
        await ledgerService.EnsureCreditAsync(request.CustomerId, cancellationToken);

        var model = await modelResolver.ResolveAsync(request.CustomerId, UsageFeature.ImageGeneration, cancellationToken);

        var result = await agentService.GenerateAsync(
            request.Prompt, request.Images, model, request.Key, cancellationToken);

        if (request.CustomerId is { } customerId)
        {
            var charge = await pricingEngine.CalculateAsync(
                new TokenUsageInput(result.InputTokens, result.CachedInputTokens, result.OutputTokens,
                    ImageCount: result.Images.Count),
                UsageFeature.ImageGeneration, model.AiModelId, cancellationToken);

            var usageRecord = new UsageRecord
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                Feature = UsageFeature.ImageGeneration,
                Model = model.Identifier,
                InputTokens = CreditGuard.ToInt(result.InputTokens),
                CachedTokens = CreditGuard.ToInt(result.CachedInputTokens),
                OutputTokens = CreditGuard.ToInt(result.OutputTokens),
                ImageCount = result.Images.Count,
                CostUsd = charge.CostUsd,
                FxRateUsed = charge.FxRateUsed,
                MultiplierUsed = charge.MultiplierUsed,
                FloorTomanUsed = charge.FloorTomanUsed,
                ChargeToman = charge.ChargeToman,
                ConversationId = null,
                CreatedAt = DateTime.Now
            };

            // A client-supplied RequestId makes a retried call idempotent; without one every
            // call is a distinct charge.
            var outcome = await ledgerService.ChargeAsync(
                customerId, charge, usageRecord,
                idempotencyKey: $"imagegen:{(request.RequestId ?? Guid.NewGuid()):N}",
                cancellationToken);

            if (outcome == ChargeOutcome.InsufficientBalance)
                throw new InsufficientCreditException();
        }

        return new ImageGenerationResponse { Images = [.. result.Images], Text = result.Text };
    }
}
