namespace SiloAI.Application.Shared.Features;

public class UpdateCustomerCommand : UpdateCustomerRequest, IRequest<CustomerDto?>
{
    public int Id { get; set; }

    public static UpdateCustomerCommand From(int id, UpdateCustomerRequest request) => new()
    {
        Id = id,
        Name = request.Name,
        StopOnInsufficientCredit = request.StopOnInsufficientCredit,
        Multiplier = request.Multiplier
    };
}
