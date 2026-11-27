namespace SiloAI.Application.Shared.Features;

/// <summary>Customers whose balance is below zero, largest debt first.</summary>
public class GetNegativeCreditCustomersQuery : IRequest<List<NegativeCreditCustomerDto>>;
