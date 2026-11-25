namespace SiloAI.Application.Api.Features;

internal static class CustomerMapping
{
    public static CustomerDto ToDto(Customer customer, decimal balanceToman) => new()
    {
        Id = customer.Id,
        Name = customer.Name,
        BalanceToman = balanceToman,
        StopOnInsufficientCredit = customer.StopOnInsufficientCredit,
        TomanPerUsd = customer.TomanPerUsd,
        Multiplier = customer.Multiplier,
        FloorToman = customer.FloorToman,
        FloorUsd = customer.FloorUsd,
        CreatedAt = customer.CreatedAt
    };
}
