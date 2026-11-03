using SiloAI.Agent.ImageGeneration;
using SiloAI.Application.Api;
using SiloAI.Application.Api.Features;
using SiloAI.Application.Shared.Contracts.AiModels;
using SiloAI.Application.Shared.Contracts.Financial;
using SiloAI.Application.Shared.Features;
using SiloAI.Domains;
using Xunit;

namespace SiloAI.Tests;

public class ImageGenerationHandlerTests
{
    private static readonly ResolvedAiModel Model = ImageGenerationAgentServiceTests.Model();

    private static (ImageGenerationCommandHandler Handler, FakeLedger Ledger, FakeAgent Agent, FakeResolver Resolver) Create(decimal balance = 100)
    {
        var ledger = new FakeLedger { Balance = balance };
        var agent = new FakeAgent();
        var resolver = new FakeResolver();
        return (new ImageGenerationCommandHandler(agent, new FakePricing(), ledger, resolver), ledger, agent, resolver);
    }

    [Fact]
    public async Task Generates_resolves_the_ImageGeneration_feature_and_bills_the_customer()
    {
        var (handler, ledger, agent, resolver) = Create();
        var requestId = Guid.NewGuid();

        var response = await handler.Handle(new ImageGenerationCommand
        {
            Prompt = "a cat", Key = "k", CustomerId = 7, RequestId = requestId,
            Images = [new ImageGenerationInputImage { Data = [1], MediaType = "image/png" }]
        }, default);

        Assert.Single(response.Images);
        Assert.Equal(UsageFeature.ImageGeneration, resolver.LastFeature);
        Assert.Equal("a cat", agent.LastPrompt);
        Assert.Equal("k", agent.LastKey);
        Assert.Equal(1, agent.LastImageCount);

        Assert.Equal($"imagegen:{requestId:N}", ledger.LastKey);
        Assert.Equal(UsageFeature.ImageGeneration, ledger.LastRecord!.Feature);
        Assert.Equal(7, ledger.LastRecord.CustomerId);
        Assert.Equal(10, ledger.LastRecord.InputTokens);
        Assert.Equal(5, ledger.LastRecord.OutputTokens);
    }

    [Fact]
    public async Task Without_a_customer_nothing_is_charged()
    {
        var (handler, ledger, _, _) = Create();

        await handler.Handle(new ImageGenerationCommand { Prompt = "x" }, default);

        Assert.Null(ledger.LastRecord);
    }

    [Fact]
    public async Task Customer_without_balance_is_rejected_before_calling_the_model()
    {
        var (handler, _, agent, _) = Create(balance: 0);

        await Assert.ThrowsAsync<InsufficientCreditException>(
            () => handler.Handle(new ImageGenerationCommand { Prompt = "x", CustomerId = 1 }, default));

        Assert.Null(agent.LastPrompt);
    }

    [Fact]
    public async Task Insufficient_balance_at_charge_time_is_surfaced()
    {
        var (handler, ledger, _, _) = Create();
        ledger.Outcome = ChargeOutcome.InsufficientBalance;

        await Assert.ThrowsAsync<InsufficientCreditException>(
            () => handler.Handle(new ImageGenerationCommand { Prompt = "x", CustomerId = 1 }, default));
    }

    private sealed class FakeAgent : IImageGenerationAgentService
    {
        public string? LastPrompt, LastKey;
        public int LastImageCount;

        public Task<ImageGenerationAgentResult> GenerateAsync(string prompt, IReadOnlyList<ImageGenerationInputImage> images,
            ResolvedAiModel model, string? key, CancellationToken cancellationToken = default)
        {
            (LastPrompt, LastKey, LastImageCount) = (prompt, key, images.Count);
            return Task.FromResult(new ImageGenerationAgentResult(
                [new GeneratedImageDto { DataBase64 = "AQ==", MediaType = "image/png" }], null, 10, 0, 5));
        }
    }

    private sealed class FakeResolver : IAiModelResolver
    {
        public UsageFeature? LastFeature;

        public Task<ResolvedAiModel> ResolveAsync(int? customerId, UsageFeature feature, CancellationToken ct)
        {
            LastFeature = feature;
            return Task.FromResult(Model);
        }

        public Task<ResolvedAiModel> GetDefaultRagModelAsync(CancellationToken ct) => throw new NotSupportedException();
    }

    private sealed class FakePricing : IPricingEngine
    {
        public Task<ChargeResult> CalculateAsync(TokenUsageInput usage, UsageFeature feature, Guid aiModelId, CancellationToken ct)
            => Task.FromResult(new ChargeResult(0.01m, 60000m, 600m, 2m, 100m, 1200m));
    }

    private sealed class FakeLedger : ICreditLedgerService
    {
        public decimal Balance;
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
}
