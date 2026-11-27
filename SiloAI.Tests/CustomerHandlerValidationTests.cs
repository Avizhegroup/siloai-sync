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
        Multiplier = CustomerPricingDefaults.Multiplier
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
    public async Task Update_RejectsBlankName()
    {
        var c = UpdateCustomerCommand.From(1, new UpdateCustomerRequest { Name = "", Multiplier = 2m });
        await Assert.ThrowsAsync<BadRequestException>(() =>
            new UpdateCustomerCommandHandler(null!).Handle(c, default));
    }

    [Fact]
    public async Task Update_RejectsZeroMultiplier()
    {
        var c = UpdateCustomerCommand.From(1, new UpdateCustomerRequest { Name = "n", Multiplier = 0m });
        await Assert.ThrowsAsync<BadRequestException>(() =>
            new UpdateCustomerCommandHandler(null!).Handle(c, default));
    }
}
