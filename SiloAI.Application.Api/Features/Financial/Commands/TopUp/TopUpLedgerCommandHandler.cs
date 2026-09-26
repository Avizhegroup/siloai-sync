namespace SiloAI.Application.Api.Features;

public class TopUpLedgerCommandHandler(ICreditLedgerService ledgerService)
    : IRequestHandler<TopUpLedgerCommand, ChargeOutcome>
{
    public async Task<ChargeOutcome> Handle(TopUpLedgerCommand request, CancellationToken cancellationToken)
    {
        return await ledgerService.TopUpAsync(
            request.CustomerId,
            request.AmountToman,
            request.Reference,
            request.Description,
            cancellationToken);
    }
}
