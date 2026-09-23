using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SiloAI.Api.Controllers;

[ApiController]
[Route("admin/financial")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class FinancialController(IMediator mediator) : ControllerBase
{
    [HttpGet("accounts")]
    public async Task<IActionResult> GetAccounts(CancellationToken cancellationToken)
        => Ok(await mediator.Send(new GetLedgerAccountsQuery(), cancellationToken));

    [HttpGet("transactions")]
    public async Task<IActionResult> GetTransactions(
        [FromQuery] int? customerId, [FromQuery] int take, CancellationToken cancellationToken)
        => Ok(await mediator.Send(new GetLedgerTransactionsQuery
        {
            CustomerId = customerId,
            Take = take <= 0 ? 100 : take
        }, cancellationToken));

    [HttpGet("usage")]
    public async Task<IActionResult> GetUsageRecords(
        [FromQuery] int? customerId, [FromQuery] int take, CancellationToken cancellationToken)
        => Ok(await mediator.Send(new GetUsageRecordsQuery
        {
            CustomerId = customerId,
            Take = take <= 0 ? 100 : take
        }, cancellationToken));

    [HttpGet("pricing")]
    public async Task<IActionResult> GetPricingSettings(CancellationToken cancellationToken)
        => Ok(await mediator.Send(new GetPricingSettingsQuery(), cancellationToken));

    [HttpPost("pricing")]
    public async Task<IActionResult> AddPricingSetting(
        [FromBody] AddPricingSettingCommand command, CancellationToken cancellationToken)
        => Ok(await mediator.Send(command, cancellationToken));

    [HttpGet("fx-rates")]
    public async Task<IActionResult> GetFxRates(CancellationToken cancellationToken)
        => Ok(await mediator.Send(new GetFxRateSettingsQuery(), cancellationToken));

    [HttpPost("fx-rates")]
    public async Task<IActionResult> AddFxRate(
        [FromBody] AddFxRateSettingCommand command, CancellationToken cancellationToken)
        => Ok(await mediator.Send(command, cancellationToken));

    [HttpPost("topup")]
    public async Task<IActionResult> TopUp(
        [FromBody] TopUpLedgerCommand command, CancellationToken cancellationToken)
    {
        var outcome = await mediator.Send(command, cancellationToken);

        return outcome switch
        {
            ChargeOutcome.Success => Ok(new { outcome }),
            ChargeOutcome.DuplicateIgnored => Conflict(new { outcome, message = "این مرجع شارژ قبلاً ثبت شده است." }),
            _ => BadRequest(new { outcome })
        };
    }
}
