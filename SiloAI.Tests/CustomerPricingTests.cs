using SiloAI.Application.Shared.Features;
using SiloAI.Domains;
using Xunit;

namespace SiloAI.Tests;

public class CustomerPricingTests
{
    private static CustomerPricingProfile Profile(decimal rate = 100000m, decimal m = 2m, decimal ft = 1000m, decimal fu = 0.01m)
        => new(rate, m, ft, fu);

    [Fact]
    public void Charge_IsCostTimesRateTimesMultiplier_WhenAboveFloors()
    {
        var r = PricingFormula.Compute(1m, Profile());
        Assert.Equal(100000m, r.CostToman);
        Assert.Equal(200000m, r.ChargeToman);
        Assert.Equal(100000m, r.FxRateUsed);
        Assert.Equal(2m, r.MultiplierUsed);
    }

    [Fact]
    public void Charge_UsesFloorToman_WhenCostIsTiny()
    {
        var r = PricingFormula.Compute(0m, Profile(ft: 5000m, fu: 0m));
        Assert.Equal(5000m, r.ChargeToman);
    }

    [Fact]
    public void Charge_UsesFloorUsdConvertedWithCustomerRate_WhenHigher()
    {
        var r = PricingFormula.Compute(0m, Profile(rate: 200000m, ft: 1000m, fu: 0.05m));
        Assert.Equal(10000m, r.ChargeToman);
    }

    [Fact]
    public void SameUsage_ChargesDifferentCustomersDifferently()
    {
        var a = PricingFormula.Compute(1m, Profile(m: 1m));
        var b = PricingFormula.Compute(1m, Profile(m: 3m));
        Assert.True(b.ChargeToman > a.ChargeToman);
    }

    [Theory]
    [InlineData(0, 2, 0, 0, false)]
    [InlineData(100000, 0, 0, 0, false)]
    [InlineData(100000, 2, -1, 0, false)]
    [InlineData(100000, 2, 0, -1, false)]
    [InlineData(100000, 2, 0, 0, true)]
    [InlineData(100000, 1.5, 1000, 0.01, true)]
    public void Validate_RejectsUnusableProfiles(decimal rate, decimal m, decimal ft, decimal fu, bool ok)
        => Assert.Equal(ok, CustomerPricingProfile.Validate(rate, m, ft, fu) is null);

    [Fact]
    public void NewCustomer_StartsWithDefaultProfile()
    {
        var c = new Customer();
        Assert.Equal(CustomerPricingDefaults.TomanPerUsd, c.TomanPerUsd);
        Assert.Equal(CustomerPricingDefaults.Multiplier, c.Multiplier);
        Assert.Equal(CustomerPricingDefaults.FloorToman, c.FloorToman);
        Assert.Equal(CustomerPricingDefaults.FloorUsd, c.FloorUsd);
        Assert.True(c.StopOnInsufficientCredit);
    }

    [Fact]
    public void CustomerEntity_HasNoSecondBalanceColumn()
        => Assert.Null(typeof(Customer).GetProperty("RemainingCredit"));

    [Fact]
    public void UpdateCommand_From_CopiesPricingAndPolicy_ButCarriesNoCredit()
    {
        var req = new UpdateCustomerRequest
        {
            Name = "x", StopOnInsufficientCredit = false,
            TomanPerUsd = 90000m, Multiplier = 3m, FloorToman = 10m, FloorUsd = 0.5m
        };
        var cmd = UpdateCustomerCommand.From(7, req);
        Assert.Equal(7, cmd.Id);
        Assert.Equal(3m, cmd.Multiplier);
        Assert.Equal(90000m, cmd.TomanPerUsd);
        Assert.False(cmd.StopOnInsufficientCredit);
        Assert.Null(typeof(UpdateCustomerRequest).GetProperty("RemainingCredit"));
        Assert.Null(typeof(UpdateCustomerRequest).GetProperty("BalanceToman"));
    }
}
