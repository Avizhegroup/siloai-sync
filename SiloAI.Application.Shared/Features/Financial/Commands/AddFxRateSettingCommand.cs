namespace SiloAI.Application.Shared.Features;

public class AddFxRateSettingCommand : IRequest<FxRateSettingDto>
{
    public decimal TomanPerUsd { get; set; }
    public DateTime? EffectiveFrom { get; set; }
}
