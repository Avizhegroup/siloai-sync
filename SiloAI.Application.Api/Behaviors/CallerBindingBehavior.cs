using SiloAI.Application.Shared.Contracts.Customers;

namespace SiloAI.Application.Api.Behaviors;

/// <summary>
/// Stamps the authenticated caller onto every request that asks for it, before its handler runs.
/// Whatever a client sent for these fields is overwritten, so a customer can never act as another
/// one — and controllers need no identity code at all.
/// </summary>
public class CallerBindingBehavior<TRequest, TResponse>(ICustomerContext caller)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        CallerStamp.Apply(request, caller);

        return next();
    }
}
