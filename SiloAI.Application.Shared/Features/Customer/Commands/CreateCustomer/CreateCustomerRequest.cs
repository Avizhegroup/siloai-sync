namespace SiloAI.Application.Shared.Features;

public class CreateCustomerRequest
{
    public string Name { get; set; }
    public decimal RemainingCredit { get; set; }
    public decimal PriceMultiplier { get; set; } = 1.0m;
}
