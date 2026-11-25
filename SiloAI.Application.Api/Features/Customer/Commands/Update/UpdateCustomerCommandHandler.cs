namespace SiloAI.Application.Api.Features;

/// <summary>
/// Edits a customer's name, credit policy and pricing profile. It never touches the credit: the
/// balance lives only in the ledger and changes through top-ups, adjustments and usage.
/// </summary>
public class UpdateCustomerCommandHandler(AiApiContext context) : IRequestHandler<UpdateCustomerCommand, CustomerDto?>
{
    public async Task<CustomerDto?> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new BadRequestException("نام مشتری الزامی است.");

        var problem = CustomerPricingProfile.Validate(
            request.TomanPerUsd, request.Multiplier, request.FloorToman, request.FloorUsd);
        if (problem is not null)
            throw new BadRequestException(problem);

        var customer = await context.Customers.FindAsync([request.Id], cancellationToken);

        if (customer is null)
            return null;

        customer.Name = request.Name.Trim();
        customer.StopOnInsufficientCredit = request.StopOnInsufficientCredit;
        customer.TomanPerUsd = request.TomanPerUsd;
        customer.Multiplier = request.Multiplier;
        customer.FloorToman = request.FloorToman;
        customer.FloorUsd = request.FloorUsd;

        await context.SaveChangesAsync(cancellationToken);

        var balance = await context.LedgerAccounts
            .AsNoTracking()
            .Where(a => a.CustomerId == customer.Id)
            .Select(a => a.BalanceToman)
            .SingleOrDefaultAsync(cancellationToken);

        return CustomerMapping.ToDto(customer, balance);
    }
}
