namespace SiloAI.Application.Shared.Features;

public class CreateCustomerRequest
{
    public string Name { get; set; }

    /// <summary>Starting credit in Toman. It is recorded as a ledger top-up, not stored on the customer.</summary>
    public decimal InitialCreditToman { get; set; }

    public bool StopOnInsufficientCredit { get; set; } = true;

    public decimal TomanPerUsd { get; set; } = CustomerPricingDefaults.TomanPerUsd;
    public decimal Multiplier { get; set; } = CustomerPricingDefaults.Multiplier;
    public decimal FloorToman { get; set; } = CustomerPricingDefaults.FloorToman;
    public decimal FloorUsd { get; set; } = CustomerPricingDefaults.FloorUsd;
}
