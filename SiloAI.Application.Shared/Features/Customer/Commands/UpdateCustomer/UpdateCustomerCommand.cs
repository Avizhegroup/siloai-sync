namespace SiloAI.Application.Shared.Features;

public class UpdateCustomerCommand : IRequest<CustomerDto?>
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal RemainingCredit { get; set; }
    public decimal PriceMultiplier { get; set; }
}
