namespace SiloAI.Application.Api.Features;

/// <summary>
/// Pricing settings are historical: a change inserts a new row with its own
/// EffectiveFrom instead of updating the old row, so past charges stay reproducible.
/// </summary>
public class AddPricingSettingCommandHandler(AiApiContext context)
    : IRequestHandler<AddPricingSettingCommand, PricingSettingDto>
{
    public async Task<PricingSettingDto> Handle(AddPricingSettingCommand request, CancellationToken cancellationToken)
    {
        var setting = new PricingSetting
        {
            Id = Guid.NewGuid(),
            Feature = request.Feature,
            Multiplier = request.Multiplier,
            FloorToman = request.FloorToman,
            FloorUsd = request.FloorUsd,
            EffectiveFrom = request.EffectiveFrom ?? DateTime.UtcNow
        };

        context.PricingSettings.Add(setting);
        await context.SaveChangesAsync(cancellationToken);

        return new PricingSettingDto
        {
            Id = setting.Id,
            Feature = setting.Feature,
            Multiplier = setting.Multiplier,
            FloorToman = setting.FloorToman,
            FloorUsd = setting.FloorUsd,
            EffectiveFrom = setting.EffectiveFrom
        };
    }
}
