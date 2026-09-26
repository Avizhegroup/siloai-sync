using Microsoft.Extensions.Options;
using SiloAI.Agent.Chat;
using SiloAI.Agent.Rag;

namespace SiloAI.Application.Api.Features;

public class OcrCommandHandler(
    ChatAgentService agentService,
    AiApiContext dbContext,
    IOptions<OpenAIOptions> openAiOptions,
    IPricingEngine pricingEngine,
    ICreditLedgerService ledgerService) : IRequestHandler<OcrCommand, OcrResponse>
{
    public async Task<OcrResponse> Handle(OcrCommand request, CancellationToken cancellationToken)
    {
        if (!await HasCreditAsync(request.CustomerId, cancellationToken))
            throw new InsufficientCreditException();

        await agentService.InitChatAgent(modelName: openAiOptions.Value.VoiceModel);

        var extractedText = await agentService.SendImageAndGetTextAsync(request.ImageData
            , request.MediaType
            , request.DocType);

        if (request.CustomerId.HasValue)
        {
            var customerId = request.CustomerId.Value;
            var model = openAiOptions.Value.VoiceModel;

            var charge = await pricingEngine.CalculateAsync(
                new TokenUsageInput(0, 0, 0),
                UsageFeature.Ocr,
                cancellationToken);

            var usageRecord = new UsageRecord
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                Feature = UsageFeature.Ocr,
                Model = model,
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
