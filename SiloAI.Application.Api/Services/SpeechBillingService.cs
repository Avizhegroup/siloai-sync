namespace SiloAI.Application.Api;

/// <summary>Pre-check + charge for the speech features, which have no handler of their own.</summary>
public interface ISpeechBillingService
{
    /// <summary>Throws <see cref="InsufficientCreditException"/> when the customer has no credit. Null customer = unmetered (admin) call.</summary>
    Task EnsureCreditAsync(int? customerId, CancellationToken cancellationToken);

    Task ChargeSpeechToTextAsync(int? customerId, ResolvedAiModel model, SpeechUsage usage, Guid requestId, CancellationToken cancellationToken);

    Task ChargeTextToSpeechAsync(int? customerId, ResolvedAiModel model, int characters, Guid requestId, CancellationToken cancellationToken);
}

public sealed class SpeechBillingService(IPricingEngine pricingEngine, ICreditLedgerService ledgerService) : ISpeechBillingService
{
    public Task EnsureCreditAsync(int? customerId, CancellationToken cancellationToken)
        => Services.CreditGuard.EnsureCreditAsync(ledgerService, customerId, cancellationToken);

    public Task ChargeSpeechToTextAsync(int? customerId, ResolvedAiModel model, SpeechUsage usage, Guid requestId, CancellationToken cancellationToken)
        => ChargeAsync(customerId, UsageFeature.SpeechToText, model, "stt", requestId,
            new TokenUsageInput(usage.InputTokens, 0, usage.OutputTokens, AudioSeconds: usage.AudioSeconds),
            characters: 0, cancellationToken);

    public Task ChargeTextToSpeechAsync(int? customerId, ResolvedAiModel model, int characters, Guid requestId, CancellationToken cancellationToken)
        => ChargeAsync(customerId, UsageFeature.TextToSpeech, model, "tts", requestId,
            new TokenUsageInput(0, 0, 0, Characters: characters), characters, cancellationToken);

    private async Task ChargeAsync(
        int? customerId, UsageFeature feature, ResolvedAiModel model, string keyPrefix, Guid requestId,
        TokenUsageInput usage, int characters, CancellationToken cancellationToken)
    {
        if (customerId is not { } id)
            return;

        var charge = await pricingEngine.CalculateAsync(usage, feature, model.AiModelId, cancellationToken);

        var record = new UsageRecord
        {
            Id = Guid.NewGuid(),
            CustomerId = id,
            Feature = feature,
            Model = model.Identifier,
            InputTokens = Services.CreditGuard.ToInt(usage.InputTokens),
            CachedTokens = 0,
            OutputTokens = Services.CreditGuard.ToInt(usage.OutputTokens),
            AudioSeconds = usage.AudioSeconds,
            Characters = characters,
            CostUsd = charge.CostUsd,
            FxRateUsed = charge.FxRateUsed,
            MultiplierUsed = charge.MultiplierUsed,
            FloorTomanUsed = charge.FloorTomanUsed,
            ChargeToman = charge.ChargeToman,
            CreatedAt = DateTime.Now
        };

        var outcome = await ledgerService.ChargeAsync(id, charge, record, $"{keyPrefix}:{requestId:N}", cancellationToken);

        if (outcome == ChargeOutcome.InsufficientBalance)
            throw new InsufficientCreditException();
    }
}
