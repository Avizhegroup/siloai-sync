using SiloAI.Application.Shared.Features;
using SiloAI.Domains;
using Xunit;

namespace SiloAI.Tests;

public class CustomerPricingTests
{
    [Fact]
    public void Charge_IsCostTimesGlobalRateTimesMultiplier()
    {
        var r = PricingFormula.Compute(1m, 100000m, 2m);
        Assert.Equal(100000m, r.CostToman);
        Assert.Equal(200000m, r.ChargeToman);
        Assert.Equal(100000m, r.FxRateUsed);
        Assert.Equal(2m, r.MultiplierUsed);
    }

    [Fact]
    public void Charge_HasNoMinimum_SoZeroCostChargesZero()
    {
        var r = PricingFormula.Compute(0m, 100000m, 2m);
        Assert.Equal(0m, r.ChargeToman);
        Assert.Equal(0m, r.FloorTomanUsed);
    }

    [Fact]
    public void SameUsageAndRate_ChargesCustomersByTheirOwnMultiplier()
    {
        var a = PricingFormula.Compute(1m, 100000m, 1m);
        var b = PricingFormula.Compute(1m, 100000m, 3m);
        Assert.Equal(100000m, a.ChargeToman);
        Assert.Equal(300000m, b.ChargeToman);
    }

    [Fact]
    public void ChangingTheGlobalRate_ChangesEveryCustomersCharge()
    {
        Assert.True(PricingFormula.Compute(1m, 120000m, 2m).ChargeToman > PricingFormula.Compute(1m, 100000m, 2m).ChargeToman);
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(-1, false)]
    [InlineData(0.5, true)]
    [InlineData(2, true)]
    public void ValidateMultiplier_RejectsNonPositive(decimal m, bool ok)
        => Assert.Equal(ok, PricingFormula.ValidateMultiplier(m) is null);

    [Fact]
    public void NewCustomer_StartsWithDefaultMultiplierAndStopPolicy()
    {
        var c = new Customer();
        Assert.Equal(CustomerPricingDefaults.Multiplier, c.Multiplier);
        Assert.True(c.StopOnInsufficientCredit);
    }

    [Fact]
    public void Customer_HasOnlyTheMultiplierAsPricing_AndNoSecondBalance()
    {
        foreach (var name in new[] { "RemainingCredit", "TomanPerUsd", "FloorToman", "FloorUsd" })
            Assert.Null(typeof(Customer).GetProperty(name));
        Assert.NotNull(typeof(Customer).GetProperty("Multiplier"));
    }

    [Fact]
    public void FxRate_IsAGlobalEntity()
        => Assert.NotNull(typeof(FxRateSetting).GetProperty("TomanPerUsd"));

    [Fact]
    public void CustomerContracts_CarryMultiplierButNoRateFloorsOrCredit()
    {
        foreach (var t in new[] { typeof(UpdateCustomerRequest), typeof(CreateCustomerRequest), typeof(CustomerDto) })
            foreach (var name in new[] { "TomanPerUsd", "FloorToman", "FloorUsd", "RemainingCredit" })
                Assert.Null(t.GetProperty(name));
        Assert.Null(typeof(UpdateCustomerRequest).GetProperty("BalanceToman"));
    }

    [Fact]
    public void UpdateCommand_From_CopiesPolicyAndMultiplier()
    {
        var cmd = UpdateCustomerCommand.From(7, new UpdateCustomerRequest { Name = "x", StopOnInsufficientCredit = false, Multiplier = 3m });
        Assert.Equal(7, cmd.Id);
        Assert.Equal(3m, cmd.Multiplier);
        Assert.False(cmd.StopOnInsufficientCredit);
    }
}
