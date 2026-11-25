using SiloAI.Application.Api.Features;
using SiloAI.Application.Shared;
using SiloAI.Application.Shared.Features;
using SiloAI.Domains;
using Xunit;

namespace SiloAI.Tests;

/// <summary>Validation runs before any database access, so no collaborators are needed.</summary>
public class CustomerHandlerValidationTests
{
    private static CreateCustomerCommand Valid() => new()
    {
        Name = "c", InitialCreditToman = 0,
        TomanPerUsd = CustomerPricingDefaults.TomanPerUsd,
        Multiplier = CustomerPricingDefaults.Multiplier,
        FloorToman = CustomerPricingDefaults.FloorToman,
        FloorUsd = CustomerPricingDefaults.FloorUsd
    };

    [Fact]
    public async Task Create_RejectsBlankName()
    {
        var c = Valid(); c.Name = " ";
        await Assert.ThrowsAsync<BadRequestException>(() =>
            new CreateCustomerCommandHandler(null!, null!).Handle(c, default));
    }

    [Fact]
    public async Task Create_RejectsNegativeInitialCredit()
    {
        var c = Valid(); c.InitialCreditToman = -1;
        await Assert.ThrowsAsync<BadRequestException>(() =>
            new CreateCustomerCommandHandler(null!, null!).Handle(c, default));
    }

    [Fact]
    public async Task Create_RejectsZeroMultiplier()
    {
        var c = Valid(); c.Multiplier = 0;
        await Assert.ThrowsAsync<BadRequestException>(() =>
            new CreateCustomerCommandHandler(null!, null!).Handle(c, default));
    }

    [Fact]
    public async Task Create_RejectsZeroRate()
    {
        var c = Valid(); c.TomanPerUsd = 0;
        await Assert.ThrowsAsync<BadRequestException>(() =>
            new CreateCustomerCommandHandler(null!, null!).Handle(c, default));
    }
}
