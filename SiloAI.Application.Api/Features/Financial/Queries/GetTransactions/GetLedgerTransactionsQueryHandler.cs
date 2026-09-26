namespace SiloAI.Application.Api.Features;

public class GetLedgerTransactionsQueryHandler(AiApiContext context)
    : IRequestHandler<GetLedgerTransactionsQuery, List<LedgerTransactionDto>>
{
    public async Task<List<LedgerTransactionDto>> Handle(GetLedgerTransactionsQuery request, CancellationToken cancellationToken)
    {
        var take = request.Take <= 0 ? 100 : Math.Clamp(request.Take, 1, 500);

        var query = context.LedgerTransactions
            .AsNoTracking()
            .AsQueryable();

        if (request.CustomerId.HasValue)
            query = query.Where(t => t.Account.CustomerId == request.CustomerId.Value);

        return await query
            .OrderByDescending(t => t.CreatedAt)
            .Take(take)
            .Select(t => new LedgerTransactionDto
            {
                Id = t.Id,
                AccountId = t.AccountId,
                CustomerId = t.Account.CustomerId,
                CustomerName = t.Account.Customer.Name,
                Type = t.Type,
                Amount = t.Amount,
                BalanceAfter = t.BalanceAfter,
                IdempotencyKey = t.IdempotencyKey,
                Description = t.Description,
                CreatedAt = t.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }
}
