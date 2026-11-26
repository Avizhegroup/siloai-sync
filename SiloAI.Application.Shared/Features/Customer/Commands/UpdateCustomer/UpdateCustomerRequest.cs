namespace SiloAI.Application.Shared.Features;

/// <summary>
/// Editable customer fields. The credit is deliberately not here: it is changed only through the
/// ledger (top-up / adjustment on the Financial page).
/// </summary>
public class UpdateCustomerRequest
{
    public string Name { get; set; }
    public bool StopOnInsufficientCredit { get; set; } = true;

    public decimal Multiplier { get; set; } = CustomerPricingDefaults.Multiplier;
}
