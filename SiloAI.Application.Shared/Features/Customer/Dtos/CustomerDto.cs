namespace SiloAI.Application.Shared.Features;

public class CustomerDto
{
    public int Id { get; set; }
    public string Name { get; set; }

    /// <summary>
    /// The customer's remaining credit in Toman — read straight from the ledger account, the one and
    /// only place a balance is kept. Read-only: it changes through top-ups, adjustments and usage on
    /// the Financial page, never by editing the customer.
    /// </summary>
    public decimal BalanceToman { get; set; }

    /// <summary>true: usage stops when credit runs out. false: the balance may go negative.</summary>
    public bool StopOnInsufficientCredit { get; set; } = true;

    // Pricing profile of this customer (nothing about pricing is global).
    public decimal TomanPerUsd { get; set; } = CustomerPricingDefaults.TomanPerUsd;
    public decimal Multiplier { get; set; } = CustomerPricingDefaults.Multiplier;
    public decimal FloorToman { get; set; } = CustomerPricingDefaults.FloorToman;
    public decimal FloorUsd { get; set; } = CustomerPricingDefaults.FloorUsd;

    public DateTime CreatedAt { get; set; }
}
