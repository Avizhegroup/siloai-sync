using SiloAI.Application.Shared.Contracts.Customers;

namespace SiloAI.Api.Infrastructure;

/// <summary>
/// <see cref="ICustomerContext"/> for HTTP requests: the identity is read once per request from the
/// authenticated principal by <see cref="CallerIdentity"/> (the only claim-parsing code).
/// </summary>
public sealed class HttpCustomerContext(IHttpContextAccessor accessor) : ICustomerContext
{
    private CallerIdentity? _identity;

    private CallerIdentity Identity => _identity ??= CallerIdentity.FromClaims(accessor.HttpContext?.User);

    public int? CustomerId => Identity.CustomerId;
    public string OwnerId => Identity.OwnerId;
    public string Username => Identity.Username;
}
