namespace SiloAI.Application.Api.Features;

public class GetNegativeCreditCustomersQueryHandler(AiApiContext context)
    : IRequestHandler<GetNegativeCreditCustomersQuery, List<NegativeCreditCustomerDto>>
{
    public async Task<List<NegativeCreditCustomerDto>> Handle(
        GetNegativeCreditCustomersQuery request, CancellationToken cancellationToken)
    {
        return await context.LedgerAccounts
            .AsNoTracking()
            .Where(a => a.BalanceToman < 0)
            .OrderBy(a => a.BalanceToman)
            .Select(a => new NegativeCreditCustomerDto
            {
                CustomerId = a.CustomerId,
                CustomerName = a.Customer.Name,
                BalanceToman = a.BalanceToman,
                StopOnInsufficientCredit = a.Customer.StopOnInsufficientCredit,
                UpdatedAt = a.UpdatedAt
            })
            .ToListAsync(cancellationToken);
    }
}
