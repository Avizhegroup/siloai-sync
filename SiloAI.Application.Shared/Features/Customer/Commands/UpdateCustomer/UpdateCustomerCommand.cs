namespace SiloAI.Application.Shared.Features;

public class UpdateCustomerCommand : UpdateCustomerRequest, IRequest<CustomerDto?>
{
    public int Id { get; set; }

    public static UpdateCustomerCommand From(int id, UpdateCustomerRequest request) => new()
    {
        Id = id,
        Name = request.Name,
        StopOnInsufficientCredit = request.StopOnInsufficientCredit,
        TomanPerUsd = request.TomanPerUsd,
        Multiplier = request.Multiplier,
        FloorToman = request.FloorToman,
        FloorUsd = request.FloorUsd
    };
}
