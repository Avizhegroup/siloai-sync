namespace SiloAI.Application.Shared.Features;

public class CustomerUsageRow
{
    public string Name { get; set; } = string.Empty;
    public decimal Toman { get; set; }
    public decimal Usd { get; set; }
    public decimal CreditUsd { get; set; }
    public decimal BalanceToman { get; set; }
    public decimal Share { get; set; }
    public string Color { get; set; } = string.Empty;
}
