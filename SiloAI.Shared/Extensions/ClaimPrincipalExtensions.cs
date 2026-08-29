using DocumentFormat.OpenXml.Spreadsheet;
using System.Security.Claims;

namespace SiloAI.Shared;

public static class ClaimsPrincipalExtensions
{
    public static string GetUserId(this ClaimsPrincipal principal)
    {
        var claims = ((ClaimsIdentity)principal.Identity).Claims;

        if (claims.Any())
        {
            return claims.Skip(1).First().Value;
        }

        return null;
    }

    public static string GetCustomerId(this ClaimsPrincipal principal)
    {
        var claims = ((ClaimsIdentity)principal.Identity).Claims;

        if (claims.Any())
        {
            return claims.FirstOrDefault(c => c.Type == "CustomerId")?.Value;
        }

        return null;
    }

    public static string GetOwnerId(this ClaimsPrincipal principal)
    {
        var customerId = principal.Claims.FirstOrDefault(c => c.Type == "CustomerId")?.Value;

        if (customerId.HasValue())
        {
            return $"customer:{customerId}";
        }

        var userId = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        if (userId.HasValue())
        {
            return $"user:{userId}";
        }

        return principal?.Identity?.Name;
    }

    public static string GetUsername(this ClaimsPrincipal principal)
    {
        var claims = ((ClaimsIdentity)principal.Identity).Claims;

        if (claims.Any())
        {
            return claims.First().Value;
        }

        return null;
    }

    public static string GetUserPersianName(this ClaimsPrincipal principal)
    {
        var claims = ((ClaimsIdentity)principal.Identity).Claims;

        var claim = claims.FirstOrDefault(p => p.Type == "family_name");

        if (claim is not null)
        {
            return claim.Value;
        }

        return null;
    }

    public static string GetUserRoleName(this ClaimsPrincipal principal)
    {
        var claims = ((ClaimsIdentity)principal.Identity).Claims;

        var claim = claims.FirstOrDefault(p => p.Type == "role");

        if (claim is not null)
        {
            return claim.Value;
        }

        return null;
    }

    public static string GetUserImage(this ClaimsPrincipal principal)
    {
        var claims = ((ClaimsIdentity)principal.Identity).Claims;

        var claim = claims.FirstOrDefault(p => p.Type == ClaimTypes.Locality);

        if (claim is not null)
        {
            return claim.Value;
        }

        return null;
    }
}
