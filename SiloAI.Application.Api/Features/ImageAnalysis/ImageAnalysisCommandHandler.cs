using SiloAI.Agent.Tasks;

namespace SiloAI.Application.Api.Features;

public class ImageAnalysisCommandHandler(
    IImageAnalysisService analysisService,
    IPricingEngine pricingEngine,
    ICreditLedgerService ledgerService,
    IAiModelResolver modelResolver) : IRequestHandler<ImageAnalysisCommand, ImageAnalysisResponse>
{
    public async Task<ImageAnalysisResponse> Handle(ImageAnalysisCommand request, CancellationToken cancellationToken)
    {
        var model = await modelResolver.ResolveAsync(request.CustomerId, UsageFeature.ImageAnalysis, cancellationToken);

        var result = await analysisService.AnalyzeImagesAsync(
            [.. request.Images.Select(i => new ImageAnalysisInput(i.Data, i.MediaType))],
            request.Prompt, model, RagDocType.ImageAnalysis, request.Key, cancellationToken);

        if (request.CustomerId is { } customerId)
        {
            var charge = await pricingEngine.CalculateAsync(
                new TokenUsageInput(result.InputTokens, result.CachedInputTokens, result.OutputTokens),
                UsageFeature.ImageAnalysis, model.AiModelId, cancellationToken);

            var usageRecord = new UsageRecord
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                Feature = UsageFeature.ImageAnalysis,
                Model = model.Identifier,
                InputTokens = (int)Math.Min(int.MaxValue, result.InputTokens),
                CachedTokens = (int)Math.Min(int.MaxValue, result.CachedInputTokens),
                OutputTokens = (int)Math.Min(int.MaxValue, result.OutputTokens),
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
                idempotencyKey: $"imageanalysis:{(request.RequestId ?? Guid.NewGuid()):N}",
                cancellationToken);

            if (outcome == ChargeOutcome.InsufficientBalance)
                throw new InsufficientCreditException();
        }

        return new ImageAnalysisResponse
        {
            Text = result.Text,
            TokenUsage = new ChatTokenUsageDto
            {
                InputTokenCount = result.InputTokens,
                CachedInputTokenCount = result.CachedInputTokens,
                OutputTokenCount = result.OutputTokens,
                TotalTokenCount = result.InputTokens + result.OutputTokens
            }
        };
    }
}
