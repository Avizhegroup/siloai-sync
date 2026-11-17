using SiloAI.Agent.Tasks;
using SiloAI.Application.Api;
using SiloAI.Application.Api.Features;
using SiloAI.Application.Shared.Contracts.AiModels;
using SiloAI.Application.Shared.Contracts.Financial;
using SiloAI.Application.Shared.Features;
using SiloAI.Domains;
using Xunit;

namespace SiloAI.Tests;

public class ImageAnalysisModelValidationTests
{
    private static ResolvedAiModel Model(bool imageIn, bool textOut) => new(
        Guid.NewGuid(), "vision-model", AiModelKind.Normal, null,
        true, textOut, imageIn, false, false, false, false, false, 1m, 2m, 0.5m);

    [Fact]
    public void Model_needs_image_input_and_text_output()
    {
        Assert.Throws<InvalidOperationException>(() => AgentFileTaskService.ValidateAnalysisModel(Model(false, true)));
        Assert.Throws<InvalidOperationException>(() => AgentFileTaskService.ValidateAnalysisModel(Model(true, false)));
        AgentFileTaskService.ValidateAnalysisModel(Model(true, true)); // ok
    }
}

public class ImageAnalysisHandlerTests
{
    private static readonly ResolvedAiModel Model = new(
        Guid.NewGuid(), "vision-model", AiModelKind.Normal, null,
        true, true, true, false, false, false, false, false, 1m, 2m, 0.5m);

    private static (ImageAnalysisCommandHandler Handler, FakeLedger Ledger, FakeAnalysis Analysis, FakeResolver Resolver) Create()
    {
        var ledger = new FakeLedger();
        var analysis = new FakeAnalysis();
        var resolver = new FakeResolver();
        return (new ImageAnalysisCommandHandler(analysis, new FakePricing(), ledger, resolver), ledger, analysis, resolver);
    }

    [Fact]
    public async Task Analyzes_with_the_ImageAnalysis_feature_and_bills_real_token_usage()
    {
        var (handler, ledger, analysis, resolver) = Create();
        var requestId = Guid.NewGuid();

        var response = await handler.Handle(new ImageAnalysisCommand
        {
            Prompt = "list defects", Key = "qa", CustomerId = 7, RequestId = requestId,
            Images = [new ImageAnalysisInputImage { Data = [1], MediaType = "image/png" },
                      new ImageAnalysisInputImage { Data = [2], MediaType = "image/jpeg" }]
        }, default);

        Assert.Equal("analysis text", response.Text);
        Assert.Equal((120L, 20L, 30L, 150L),
            (response.TokenUsage.InputTokenCount, response.TokenUsage.CachedInputTokenCount,
             response.TokenUsage.OutputTokenCount, response.TokenUsage.TotalTokenCount));

        Assert.Equal(UsageFeature.ImageAnalysis, resolver.LastFeature);
        Assert.Equal(RagDocType.ImageAnalysis, analysis.LastDocType);
        Assert.Equal("list defects", analysis.LastPrompt);
        Assert.Equal("qa", analysis.LastKey);
        Assert.Equal(2, analysis.LastImageCount);

        Assert.Equal($"imageanalysis:{requestId:N}", ledger.LastKey);
        Assert.Equal(UsageFeature.ImageAnalysis, ledger.LastRecord!.Feature);
        Assert.Equal(7, ledger.LastRecord.CustomerId);
        Assert.Equal(120, ledger.LastRecord.InputTokens);
        Assert.Equal(30, ledger.LastRecord.OutputTokens);
    }

    [Fact]
    public async Task Without_a_customer_nothing_is_charged()
    {
        var (handler, ledger, _, _) = Create();

        await handler.Handle(new ImageAnalysisCommand
        {
            Images = [new ImageAnalysisInputImage { Data = [1], MediaType = "image/png" }]
        }, default);

        Assert.Null(ledger.LastRecord);
    }

    [Fact]
    public async Task Insufficient_balance_at_charge_time_is_surfaced()
    {
        var (handler, ledger, _, _) = Create();
        ledger.Outcome = ChargeOutcome.InsufficientBalance;

        await Assert.ThrowsAsync<InsufficientCreditException>(() => handler.Handle(new ImageAnalysisCommand
        {
            CustomerId = 1, Images = [new ImageAnalysisInputImage { Data = [1], MediaType = "image/png" }]
        }, default));
    }

    private sealed class FakeAnalysis : IImageAnalysisService
    {
        public string? LastPrompt, LastKey;
        public RagDocType? LastDocType;
        public int LastImageCount;

        public Task<ImageAnalysisResult> AnalyzeImagesAsync(IReadOnlyList<ImageAnalysisInput> images, string? prompt,
            ResolvedAiModel model, RagDocType docType, string? key, CancellationToken cancellationToken = default)
        {
            (LastPrompt, LastKey, LastDocType, LastImageCount) = (prompt, key, docType, images.Count);
            return Task.FromResult(new ImageAnalysisResult("analysis text", 120, 20, 30));
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

        public Task<decimal> GetBalanceAsync(int customerId, CancellationToken ct) => Task.FromResult(100m);
    }
}
