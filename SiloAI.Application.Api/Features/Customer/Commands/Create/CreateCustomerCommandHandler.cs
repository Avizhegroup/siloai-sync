namespace SiloAI.Application.Api.Features;

public class CreateCustomerCommandHandler(AiApiContext context, ICreditLedgerService ledgerService)
    : IRequestHandler<CreateCustomerCommand, CustomerDto>
{
    public async Task<CustomerDto> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new BadRequestException("نام مشتری الزامی است.");

        if (request.InitialCreditToman < 0)
            throw new BadRequestException("اعتبار اولیه نمی‌تواند منفی باشد.");

        var problem = CustomerPricingProfile.Validate(
            request.TomanPerUsd, request.Multiplier, request.FloorToman, request.FloorUsd);
        if (problem is not null)
            throw new BadRequestException(problem);

        var customer = new Customer
        {
            Name = request.Name.Trim(),
            StopOnInsufficientCredit = request.StopOnInsufficientCredit,
            TomanPerUsd = request.TomanPerUsd,
            Multiplier = request.Multiplier,
            FloorToman = request.FloorToman,
            FloorUsd = request.FloorUsd,
            CreatedAt = DateTime.Now
        };

        context.Customers.Add(customer);
        await context.SaveChangesAsync(cancellationToken);

        // Every customer owns exactly one ledger account from the moment of creation.
        var createdAt = DateTime.Now;

        context.LedgerAccounts.Add(new LedgerAccount
        {
            Id = Guid.NewGuid(),
            CustomerId = customer.Id,
            BalanceToman = 0,
            CreatedAt = createdAt,
            UpdatedAt = createdAt
        });

        await context.SaveChangesAsync(cancellationToken);

        var balance = 0m;

        if (request.InitialCreditToman > 0)
        {
            // The initial credit goes through the ledger so SUM(transactions) == balance.
            var outcome = await ledgerService.TopUpAsync(
                customer.Id, request.InitialCreditToman,
                reference: $"customer-create:{customer.Id}",
                description: "اعتبار اولیه",
                cancellationToken);

            if (outcome == ChargeOutcome.InsufficientBalance)
                throw new InvalidOperationException("Failed to record the initial credit of the new customer.");

            balance = request.InitialCreditToman;
        }

        return CustomerMapping.ToDto(customer, balance);
    }
}
