namespace SiloAI.Application.Api.Features;

public class GetLedgerAccountsQueryHandler(AiApiContext context)
    : IRequestHandler<GetLedgerAccountsQuery, List<LedgerAccountDto>>
{
    public async Task<List<LedgerAccountDto>> Handle(GetLedgerAccountsQuery request, CancellationToken cancellationToken)
    {
        return await context.LedgerAccounts
            .AsNoTracking()
            .OrderByDescending(a => a.UpdatedAt)
            .Select(a => new LedgerAccountDto
            {
                Id = a.Id,
                CustomerId = a.CustomerId,
                CustomerName = a.Customer.Name,
                BalanceToman = a.BalanceToman,
                UpdatedAt = a.UpdatedAt
            })
            .ToListAsync(cancellationToken);
    }
}
