namespace SiloAI.Application.Shared.Features;

/// <summary>
/// The full 4-feature assignment grid for one customer (CustomerId = null means "the global
/// defaults themselves"), each row showing whether it's the customer's own override or falls
/// through to the default.
/// </summary>
public class GetModelAssignmentsQuery : IRequest<List<ModelAssignmentDto>>
{
    public int? CustomerId { get; set; }
}

public class SetModelAssignmentCommand : IRequest<ModelAssignmentDto>
{
    public SetModelAssignmentRequest Request { get; set; }
}

/// <summary>Removes a customer's override for a feature, reverting them to the global default.</summary>
public class ClearModelAssignmentCommand : IRequest<bool>
{
    public int CustomerId { get; set; }
    public UsageFeature Feature { get; set; }
}
