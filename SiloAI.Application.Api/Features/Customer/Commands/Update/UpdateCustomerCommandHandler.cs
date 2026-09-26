namespace SiloAI.Application.Api.Features;

public class UpdateCustomerCommandHandler(AiApiContext context) : IRequestHandler<UpdateCustomerCommand, CustomerDto?>
{
    public async Task<CustomerDto?> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await context.Customers.FindAsync([request.Id], cancellationToken);

        if (customer is null)
            return null;

        var oldCreditUsd = customer.RemainingCredit;
        var newCreditUsd = request.RemainingCredit;

        customer.Name = request.Name;
        customer.RemainingCredit = newCreditUsd;

        // Mirror the credit change into the ledger (Toman) as an Adjustment so the
        // account balance never diverges silently from the legacy credit column.
        if (newCreditUsd != oldCreditUsd)
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
                    "No active FX rate found. Seed tbl_FxRateSettings with a row whose EffectiveFrom <= now before changing customer credit.");

            var account = await context.LedgerAccounts
                .SingleOrDefaultAsync(a => a.CustomerId == customer.Id, cancellationToken);

            if (account is not null)
            {
                var deltaToman = (newCreditUsd - oldCreditUsd) * fxRate;
                var newBalanceToman = account.BalanceToman + deltaToman;

                if (newBalanceToman < 0)
                    throw new InvalidOperationException(
                        $"Credit reduction would make the ledger balance negative ({newBalanceToman:N0} Toman).");

                account.BalanceToman = newBalanceToman;
                account.UpdatedAt = now;

                context.LedgerTransactions.Add(new LedgerTransaction
                {
                    Id = Guid.NewGuid(),
                    AccountId = account.Id,
                    Type = LedgerTransactionType.Adjustment,
                    Amount = deltaToman,
                    BalanceAfter = newBalanceToman,
                    IdempotencyKey = $"customer-adjust:{customer.Id}:{Guid.NewGuid():N}",
                    Description = "تعدیل اعتبار از پنل مدیریت",
                    CreatedAt = now
                });
            }
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

