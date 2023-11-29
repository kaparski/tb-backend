using System.Security.Claims;

namespace TaxBeacon.API.Authentication;

public static class ClaimsPrincipalExtensions
{
    public static bool HasAnyPermission<T>(this ClaimsPrincipal claimsPrincipal, T[] permissions)
        where T : struct, IConvertible
    {
        var transformedPermission = permissions
            .Select(p => $"{p.GetType().Name}.{p}")
            .ToArray()
            .AsReadOnly();

        return HasAnyPermission(claimsPrincipal, transformedPermission);
    }

    public static bool HasAnyPermission(this ClaimsPrincipal claimsPrincipal,
        IReadOnlyCollection<string> permissions) =>
        claimsPrincipal
            .FindAll(p => p.Type.Equals(Claims.Permission, StringComparison.OrdinalIgnoreCase))
            .Select(p => p.Value)
            .Intersect(permissions)
            .Any();

    public static string? GetEmail(this ClaimsPrincipal claimsPrincipal) =>
        claimsPrincipal
            .Claims
            .SingleOrDefault(claim =>
                claim.Type.Equals(Claims.EmailClaimName, StringComparison.OrdinalIgnoreCase))
            ?.Value
        ??
        claimsPrincipal
            .Claims
            .SingleOrDefault(claim =>
                claim.Type.Equals(Claims.OtherMails, StringComparison.OrdinalIgnoreCase))
            ?.Value;
}
