namespace SiloAI.Application.Api.Features;

public class GetPricingSettingsQueryHandler(AiApiContext context)
    : IRequestHandler<GetPricingSettingsQuery, List<PricingSettingDto>>
{
    public async Task<List<PricingSettingDto>> Handle(GetPricingSettingsQuery request, CancellationToken cancellationToken)
    {
        return await context.PricingSettings
            .AsNoTracking()
            .OrderBy(p => p.Feature)
            .ThenByDescending(p => p.EffectiveFrom)
            .Select(p => new PricingSettingDto
            {
                Id = p.Id,
                Feature = p.Feature,
                Multiplier = p.Multiplier,
                FloorToman = p.FloorToman,
                FloorUsd = p.FloorUsd,
                EffectiveFrom = p.EffectiveFrom
            })
            .ToListAsync(cancellationToken);
    }
}
