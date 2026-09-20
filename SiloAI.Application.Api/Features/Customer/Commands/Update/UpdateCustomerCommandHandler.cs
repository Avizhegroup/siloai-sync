namespace SiloAI.Application.Api.Features;

public class UpdateCustomerCommandHandler(AiApiContext context) : IRequestHandler<UpdateCustomerCommand, CustomerDto?>
{
    public async Task<CustomerDto?> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await context.Customers.FindAsync([request.Id], cancellationToken);

        if (customer is null)
            return null;

        customer.Name = request.Name;
        customer.RemainingCredit = request.RemainingCredit;
        customer.PriceMultiplier = request.PriceMultiplier;
        await context.SaveChangesAsync(cancellationToken);

        return new CustomerDto
        {
            Id = customer.Id,
            Name = customer.Name,
            RemainingCredit = customer.RemainingCredit,
            CreatedAt = customer.CreatedAt,
            PriceMultiplier = customer.PriceMultiplier
        };
    }
}
