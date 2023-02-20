using Microsoft.AspNetCore.Authorization;

namespace TaxBeacon.API.Extensions.AuthorizationServices;

public class PermissionAuthorizationHandler: AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        string? userId = context.User.Claims
    }
}
