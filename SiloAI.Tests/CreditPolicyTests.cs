using SiloAI.Application.Api;
using SiloAI.Application.Api.Services;
using SiloAI.Application.Shared.Contracts.Financial;
using SiloAI.Application.Shared.Features;
using SiloAI.Domains;
using Xunit;

namespace SiloAI.Tests;

public class CreditPolicyTests
{
    [Theory]
    [InlineData(100, true, false)]   // has credit
    [InlineData(0, true, true)]      // exhausted, stopped
    [InlineData(-5, true, true)]     // negative, stopped
    [InlineData(0, false, false)]    // exhausted but allowed to continue
    [InlineData(-500, false, false)] // already in debt but allowed to continue
    [InlineData(1, false, false)]
    public void Only_stopped_customers_with_no_credit_are_blocked(double balance, bool stop, bool blocked)
        => Assert.Equal(blocked, new CreditStatus((decimal)balance, stop).MustBlock);

    private sealed class PolicyLedger(decimal balance, bool stop) : ICreditLedgerService
    {
        public Task<ChargeOutcome> ChargeAsync(int customerId, ChargeResult charge, UsageRecord usageRecord, string idempotencyKey, CancellationToken ct)
            => throw new NotSupportedException();

        public Task<ChargeOutcome> TopUpAsync(int customerId, decimal amountToman, string reference, string? description, CancellationToken ct)
            => throw new NotSupportedException();

        public Task<decimal> GetBalanceAsync(int customerId, CancellationToken ct) => Task.FromResult(balance);

        public Task<CreditStatus> GetCreditStatusAsync(int customerId, CancellationToken ct)
            => Task.FromResult(new CreditStatus(balance, stop));
    }

    /// <summary>A ledger that only knows balances (like the older fakes): the default policy applies.</summary>
    private sealed class BalanceOnlyLedger(decimal balance) : ICreditLedgerService
    {
        public Task<ChargeOutcome> ChargeAsync(int customerId, ChargeResult charge, UsageRecord usageRecord, string idempotencyKey, CancellationToken ct)
            => throw new NotSupportedException();

        public Task<ChargeOutcome> TopUpAsync(int customerId, decimal amountToman, string reference, string? description, CancellationToken ct)
            => throw new NotSupportedException();

        public Task<decimal> GetBalanceAsync(int customerId, CancellationToken ct) => Task.FromResult(balance);
    }

    [Fact]
    public async Task A_stopped_customer_without_credit_is_refused()
        => await Assert.ThrowsAsync<InsufficientCreditException>(
            () => new PolicyLedger(0, stop: true).EnsureCreditAsync(1, default));

    [Fact]
    public async Task A_customer_allowed_to_go_negative_is_never_refused_up_front()
    {
        await new PolicyLedger(0, stop: false).EnsureCreditAsync(1, default);
        await new PolicyLedger(-12_000, stop: false).EnsureCreditAsync(1, default);
    }

    [Fact]
    public async Task A_stopped_customer_with_credit_is_allowed()
        => await new PolicyLedger(10, stop: true).EnsureCreditAsync(1, default);

    [Fact]
    public async Task Calls_without_a_customer_are_never_checked()
        => await new PolicyLedger(-1, stop: true).EnsureCreditAsync(null, default);

    [Fact]
    public async Task A_ledger_that_only_reports_balances_keeps_the_stop_behaviour()
    {
        ICreditLedgerService ledger = new BalanceOnlyLedger(0);
        var status = await ledger.GetCreditStatusAsync(1, default);

        Assert.True(status.MustBlock);
        await Assert.ThrowsAsync<InsufficientCreditException>(() => ledger.EnsureCreditAsync(1, default));
        await new BalanceOnlyLedger(5).EnsureCreditAsync(1, default);
    }

    [Fact]
    public async Task Speech_billing_honours_the_customer_credit_policy()
    {
        var pricing = new SpeechBillingTests.RecordingPricingForPolicy();

        await new SpeechBillingService(pricing, new PolicyLedger(0, stop: false)).EnsureCreditAsync(3, default);

        await Assert.ThrowsAsync<InsufficientCreditException>(
            () => new SpeechBillingService(pricing, new PolicyLedger(0, stop: true)).EnsureCreditAsync(3, default));
    }
}
