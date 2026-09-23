namespace SiloAI.Application.Shared.Features;

public class AddPricingSettingCommand : IRequest<PricingSettingDto>
{
    public UsageFeature Feature { get; set; }
    public decimal Multiplier { get; set; }
    public decimal FloorToman { get; set; }
    public decimal FloorUsd { get; set; }
    public DateTime? EffectiveFrom { get; set; }
}
