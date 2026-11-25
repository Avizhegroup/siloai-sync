namespace SiloAI.Application.Api.Features;

public class GetCustomerByIdQueryHandler(AiApiContext context) : IRequestHandler<GetCustomerByIdQuery, CustomerDto?>
{
    public async Task<CustomerDto?> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        return await context.Customers
            .AsNoTracking()
            .Where(c => c.Id == request.Id)
            .Select(c => new CustomerDto
            {
                Id = c.Id,
                Name = c.Name,
                BalanceToman = c.LedgerAccount != null ? c.LedgerAccount.BalanceToman : 0m,
                StopOnInsufficientCredit = c.StopOnInsufficientCredit,
                TomanPerUsd = c.TomanPerUsd,
                Multiplier = c.Multiplier,
                FloorToman = c.FloorToman,
                FloorUsd = c.FloorUsd,
                CreatedAt = c.CreatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
