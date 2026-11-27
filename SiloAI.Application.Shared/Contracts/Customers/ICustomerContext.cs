using System.Security.Claims;

namespace SiloAI.Application.Shared.Contracts.Customers;

/// <summary>
/// The single place that answers "who is calling": which customer (if any), the conversation-owner
/// key and the display name. Controllers, handlers and middleware use this instead of each parsing
/// claims on their own. A caller with no customer id is an admin (JWT) and is not metered.
/// </summary>
public interface ICustomerContext
{
    int? CustomerId { get; }
    string OwnerId { get; }
    string Username { get; }

    /// <summary>True when the caller is a customer (API key / customer token) rather than an admin.</summary>
    bool IsCustomer => CustomerId.HasValue;
}

/// <summary>A request whose <c>CustomerId</c> is filled in from the authenticated caller, never from the client.</summary>
public interface ICustomerBound
{
    int? CustomerId { get; set; }
}

/// <summary>A request whose conversation-owner key is filled in from the authenticated caller.</summary>
public interface IOwnerBound
{
    // Nullable on purpose: the value is filled by the pipeline after model binding, so MVC must
    // not demand it from the client body.
    string? OwnerId { get; set; }
}

/// <summary>A request whose username is filled in from the authenticated caller.</summary>
public interface IUsernameBound
{
    string? Username { get; set; }
}

/// <summary>
/// Pure claim parsing behind <see cref="ICustomerContext"/> (no HTTP dependency, so it is unit
/// testable). This is the only implementation of these rules in the code base.
/// </summary>
public sealed record CallerIdentity(int? CustomerId, string OwnerId, string Username)
{
    public const string CustomerIdClaim = "CustomerId";

    public static readonly CallerIdentity Anonymous = new(null, string.Empty, string.Empty);

    public static CallerIdentity FromClaims(ClaimsPrincipal? principal)
    {
        if (principal is null)
            return Anonymous;

        var customerClaim = principal.Claims.FirstOrDefault(c => c.Type == CustomerIdClaim)?.Value;
        int? customerId = int.TryParse(customerClaim, out var id) ? id : null;

        var username = principal.Identity?.Name ?? string.Empty;

        string ownerId;
        if (!string.IsNullOrWhiteSpace(customerClaim))
            ownerId = $"customer:{customerClaim}";
        else if (principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value is { Length: > 0 } userId)
            ownerId = $"user:{userId}";
        else
            ownerId = username;

        return new CallerIdentity(customerId, ownerId, username);
    }
}

/// <summary>Stamps the authenticated caller onto a request (pure, so it is unit-tested directly).</summary>
public static class CallerStamp
{
    public static void Apply(object request, ICustomerContext caller)
    {
        if (request is ICustomerBound customerBound) customerBound.CustomerId = caller.CustomerId;
        if (request is IOwnerBound ownerBound) ownerBound.OwnerId = caller.OwnerId;
        if (request is IUsernameBound usernameBound) usernameBound.Username = caller.Username;
    }
}
