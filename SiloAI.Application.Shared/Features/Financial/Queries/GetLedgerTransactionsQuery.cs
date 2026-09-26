namespace SiloAI.Application.Shared.Features;

public class GetLedgerTransactionsQuery : IRequest<List<LedgerTransactionDto>>
{
    public int? CustomerId { get; set; }
    public int Take { get; set; } = 100;
}
