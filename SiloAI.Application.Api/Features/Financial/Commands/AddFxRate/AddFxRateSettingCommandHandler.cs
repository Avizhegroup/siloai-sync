namespace SiloAI.Application.Api.Features;

/// <summary>
/// Exchange rates are historical: a change inserts a new row with its own
/// EffectiveFrom instead of updating the old row, so past charges stay reproducible.
/// </summary>
public class AddFxRateSettingCommandHandler(AiApiContext context)
    : IRequestHandler<AddFxRateSettingCommand, FxRateSettingDto>
{
    public async Task<FxRateSettingDto> Handle(AddFxRateSettingCommand request, CancellationToken cancellationToken)
    {
        var setting = new FxRateSetting
        {
            Id = Guid.NewGuid(),
            TomanPerUsd = request.TomanPerUsd,
            EffectiveFrom = request.EffectiveFrom ?? DateTime.UtcNow
        };

        context.FxRateSettings.Add(setting);
        await context.SaveChangesAsync(cancellationToken);

        return new FxRateSettingDto
        {
            Id = setting.Id,
            TomanPerUsd = setting.TomanPerUsd,
            EffectiveFrom = setting.EffectiveFrom
        };
    }
}
