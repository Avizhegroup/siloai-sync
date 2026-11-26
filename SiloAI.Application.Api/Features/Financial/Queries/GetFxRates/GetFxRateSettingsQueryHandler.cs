namespace SiloAI.Application.Api.Features;

public class GetFxRateSettingsQueryHandler(AiApiContext context)
    : IRequestHandler<GetFxRateSettingsQuery, List<FxRateSettingDto>>
{
    public async Task<List<FxRateSettingDto>> Handle(GetFxRateSettingsQuery request, CancellationToken cancellationToken)
    {
        return await context.FxRateSettings
            .AsNoTracking()
            .OrderByDescending(x => x.EffectiveFrom)
            .Select(x => new FxRateSettingDto
            {
                Id = x.Id,
                TomanPerUsd = x.TomanPerUsd,
                EffectiveFrom = x.EffectiveFrom
            })
            .ToListAsync(cancellationToken);
    }
}
