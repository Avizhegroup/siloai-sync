namespace SiloAI.Application.Api.Features;

public class CreateCustomerCommandHandler(AiApiContext context) : IRequestHandler<CreateCustomerCommand, CustomerDto>
{
    public async Task<CustomerDto> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = new Customer
        {
            Name = request.Name,
            RemainingCredit = request.RemainingCredit,
            CreatedAt = DateTime.Now
        };

        context.Customers.Add(customer);
        await context.SaveChangesAsync(cancellationToken);

        // Every customer owns exactly one ledger account from the moment of creation;
        // the initial credit is recorded as the first ledger transaction.
        var now = DateTime.UtcNow;

        var account = new LedgerAccount
        {
            Id = Guid.NewGuid(),
            CustomerId = customer.Id,
            BalanceToman = request.RemainingCredit,
            CreatedAt = now,
            UpdatedAt = now
        };

        context.LedgerAccounts.Add(account);

        if (request.RemainingCredit > 0)
        {
            context.LedgerTransactions.Add(new LedgerTransaction
            {
                Id = Guid.NewGuid(),
                AccountId = account.Id,
                Type = LedgerTransactionType.TopUp,
                Amount = request.RemainingCredit,
                BalanceAfter = request.RemainingCredit,
                IdempotencyKey = $"customer-create:{customer.Id}",
                Description = "اعتبار اولیه",
                CreatedAt = now
            });
        }

        await context.SaveChangesAsync(cancellationToken);

        return new CustomerDto
        {
            Id = customer.Id,
            Name = customer.Name,
            RemainingCredit = customer.RemainingCredit,
            CreatedAt = customer.CreatedAt
        };
    }
}
