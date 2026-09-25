namespace SiloAI.Application.Api.Features;

public class CreateCustomerCommandHandler(AiApiContext context, ICreditLedgerService ledgerService)
    : IRequestHandler<CreateCustomerCommand, CustomerDto>
{
    public async Task<CustomerDto> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        // The legacy RemainingCredit field (and its admin UI) is denominated in USD.
        // The ledger stores Toman, so the initial credit must be converted with the
        // active exchange rate instead of copying the raw number across units.
        var initialBalanceToman = 0m;

        if (request.RemainingCredit > 0)
        {
            var now = DateTime.UtcNow;

            var fxRate = await context.FxRateSettings
                .AsNoTracking()
                .Where(x => x.EffectiveFrom <= now)
                .OrderByDescending(x => x.EffectiveFrom)
                .Select(x => x.TomanPerUsd)
                .FirstOrDefaultAsync(cancellationToken);

            if (fxRate <= 0)
                throw new InvalidOperationException(
                    "No active FX rate found. Seed tbl_FxRateSettings with a row whose EffectiveFrom <= now before creating customers with initial credit.");

            initialBalanceToman = request.RemainingCredit * fxRate;
        }

        var customer = new Customer
        {
            Name = request.Name,
            RemainingCredit = request.RemainingCredit,
            CreatedAt = DateTime.Now
        };

        context.Customers.Add(customer);
        await context.SaveChangesAsync(cancellationToken);

        // Every customer owns exactly one ledger account from the moment of creation.
        var createdAt = DateTime.UtcNow;

        context.LedgerAccounts.Add(new LedgerAccount
        {
            Id = Guid.NewGuid(),
            CustomerId = customer.Id,
            BalanceToman = 0,
            CreatedAt = createdAt,
            UpdatedAt = createdAt
        });

        await context.SaveChangesAsync(cancellationToken);

        if (initialBalanceToman > 0)
        {
            // The initial credit goes through the ledger so SUM(transactions) == balance.
            var outcome = await ledgerService.TopUpAsync(
                customer.Id, initialBalanceToman,
                reference: $"customer-create:{customer.Id}",
                description: "اعتبار اولیه",
                cancellationToken);

            if (outcome == ChargeOutcome.InsufficientBalance)
                throw new InvalidOperationException("Failed to record the initial credit of the new customer.");
        }

        return new CustomerDto
        {
            Id = customer.Id,
            Name = customer.Name,
            RemainingCredit = customer.RemainingCredit,
            CreatedAt = customer.CreatedAt
        };
    }
}

