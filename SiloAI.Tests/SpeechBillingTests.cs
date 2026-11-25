using System.Text.Json;
using SiloAI.Application.Api;
using SiloAI.Application.Api.Services;
using SiloAI.Application.Shared.Contracts.AiModels;
using SiloAI.Application.Shared.Contracts.Financial;
using SiloAI.Application.Shared.Features;
using SiloAI.Domains;
using Xunit;

namespace SiloAI.Tests;

public class SpeechBillingTests
{
    private static readonly ResolvedAiModel Model = new(
        Guid.NewGuid(), "stt-model", AiModelKind.SpeechToText, null,
        false, false, false, false, false, false, false, false, 0m, 0m, 0m);

    /// <summary>Exposes a no-op pricing engine to other test classes.</summary>
    public sealed class RecordingPricingForPolicy : IPricingEngine
    {
        public Task<ChargeResult> CalculateAsync(int customerId, TokenUsageInput usage, UsageFeature feature, Guid aiModelId, CancellationToken ct)
            => Task.FromResult(new ChargeResult(0m, 1m, 0m, 1m, 0m, 0m));
    }

    private sealed class RecordingPricing : IPricingEngine
    {
        public TokenUsageInput? LastUsage;
        public UsageFeature? LastFeature;

        public Task<ChargeResult> CalculateAsync(int customerId, TokenUsageInput usage, UsageFeature feature, Guid aiModelId, CancellationToken ct)
        {
            (LastUsage, LastFeature) = (usage, feature);
            return Task.FromResult(new ChargeResult(0.01m, 60000m, 600m, 2m, 100m, 1200m));
        }
    }

    private sealed class Ledger : ICreditLedgerService
    {
        public decimal Balance = 100;
        public ChargeOutcome Outcome = ChargeOutcome.Success;
        public string? LastKey;
        public UsageRecord? LastRecord;

        public Task<ChargeOutcome> ChargeAsync(int customerId, ChargeResult charge, UsageRecord usageRecord, string idempotencyKey, CancellationToken ct)
        {
            (LastKey, LastRecord) = (idempotencyKey, usageRecord);
            return Task.FromResult(Outcome);
        }

        public Task<ChargeOutcome> TopUpAsync(int customerId, decimal amountToman, string reference, string? description, CancellationToken ct)
            => throw new NotSupportedException();

        public Task<decimal> GetBalanceAsync(int customerId, CancellationToken ct) => Task.FromResult(Balance);
    }

    [Fact]
    public async Task Speech_to_text_is_charged_with_audio_seconds_and_tokens()
    {
        var pricing = new RecordingPricing();
        var ledger = new Ledger();
        var billing = new SpeechBillingService(pricing, ledger);
        var id = Guid.NewGuid();

        await billing.ChargeSpeechToTextAsync(5, Model, new SpeechUsage(100, 20, 42.5m), id, default);

        Assert.Equal(UsageFeature.SpeechToText, pricing.LastFeature);
        Assert.Equal(42.5m, pricing.LastUsage!.AudioSeconds);
        Assert.Equal(100, pricing.LastUsage.InputTokens);
        Assert.Equal($"stt:{id:N}", ledger.LastKey);
        Assert.Equal(5, ledger.LastRecord!.CustomerId);
        Assert.Equal(42.5m, ledger.LastRecord.AudioSeconds);
        Assert.Equal(1200m, ledger.LastRecord.ChargeToman);
    }

    [Fact]
    public async Task Text_to_speech_is_charged_per_character()
    {
        var pricing = new RecordingPricing();
        var ledger = new Ledger();
        var billing = new SpeechBillingService(pricing, ledger);

        await billing.ChargeTextToSpeechAsync(5, Model, 321, Guid.NewGuid(), default);

        Assert.Equal(UsageFeature.TextToSpeech, pricing.LastFeature);
        Assert.Equal(321, pricing.LastUsage!.Characters);
        Assert.Equal(321, ledger.LastRecord!.Characters);
        Assert.StartsWith("tts:", ledger.LastKey);
    }

    [Fact]
    public async Task Admin_calls_without_a_customer_are_not_charged()
    {
        var ledger = new Ledger();
        var billing = new SpeechBillingService(new RecordingPricing(), ledger);

        await billing.ChargeSpeechToTextAsync(null, Model, SpeechUsage.Unknown, Guid.NewGuid(), default);
        await billing.ChargeTextToSpeechAsync(null, Model, 10, Guid.NewGuid(), default);
        await billing.EnsureCreditAsync(null, default);

        Assert.Null(ledger.LastRecord);
    }

    [Fact]
    public async Task A_customer_without_credit_is_refused_up_front()
    {
        var billing = new SpeechBillingService(new RecordingPricing(), new Ledger { Balance = 0 });

        await Assert.ThrowsAsync<InsufficientCreditException>(() => billing.EnsureCreditAsync(5, default));
    }

    [Fact]
    public async Task Insufficient_balance_at_charge_time_is_surfaced()
    {
        var billing = new SpeechBillingService(new RecordingPricing(), new Ledger { Outcome = ChargeOutcome.InsufficientBalance });

        await Assert.ThrowsAsync<InsufficientCreditException>(
            () => billing.ChargeTextToSpeechAsync(5, Model, 10, Guid.NewGuid(), default));
    }

    [Theory]
    [InlineData("""{"text":"hi","usage":{"type":"tokens","input_tokens":120,"output_tokens":30}}""", 120, 30, 0)]
    [InlineData("""{"text":"hi","usage":{"type":"duration","seconds":12.5}}""", 0, 0, 12.5)]
    [InlineData("""{"text":"hi","duration":7.25}""", 0, 0, 7.25)]
    [InlineData("""{"text":"hi"}""", 0, 0, 0)]
    public void Transcription_usage_is_read_from_every_known_response_shape(string json, long input, long output, double seconds)
    {
        using var doc = JsonDocument.Parse(json);

        var usage = TranscriptionUsageParser.Parse(doc.RootElement);

        Assert.Equal(input, usage.InputTokens);
        Assert.Equal(output, usage.OutputTokens);
        Assert.Equal((decimal)seconds, usage.AudioSeconds);
    }

    [Fact]
    public async Task Credit_guard_blocks_only_customers_with_no_balance()
    {
        await new Ledger { Balance = 1 }.EnsureCreditAsync(1, default);
        await new Ledger { Balance = 0 }.EnsureCreditAsync(null, default);
        await Assert.ThrowsAsync<InsufficientCreditException>(() => new Ledger { Balance = 0 }.EnsureCreditAsync(1, default));
    }

    [Fact]
    public void Token_counters_are_clamped_into_int_columns()
    {
        Assert.Equal(int.MaxValue, CreditGuard.ToInt(long.MaxValue));
        Assert.Equal(0, CreditGuard.ToInt(-4));
        Assert.Equal(42, CreditGuard.ToInt(42));
    }
}
