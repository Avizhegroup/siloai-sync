namespace SiloAI.Domains;

/// <summary>
/// Starting pricing profile of a new customer (pre-filled in the create form; an admin changes it
/// per customer afterwards). These match the values the system was seeded with.
/// </summary>
public static class CustomerPricingDefaults
{
    public const decimal TomanPerUsd = 100000m;
    public const decimal Multiplier = 2m;
    public const decimal FloorToman = 1000m;
    public const decimal FloorUsd = 0.01m;
}
