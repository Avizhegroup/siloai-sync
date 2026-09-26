namespace SiloAI.Application.Shared.Features;

public class TopUpLedgerCommand : IRequest<ChargeOutcome>
{
    public int CustomerId { get; set; }
    public decimal AmountToman { get; set; }
    public string Reference { get; set; }
    public string? Description { get; set; }
}
