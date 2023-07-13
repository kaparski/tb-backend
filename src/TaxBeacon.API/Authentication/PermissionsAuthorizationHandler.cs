using Microsoft.AspNetCore.Authorization;

namespace TaxBeacon.API.Authentication;

public sealed class PermissionsAuthorizationHandler: AuthorizationHandler<PermissionsRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionsRequirement requirement)
    {
        if (context.User.HasAnyPermission(requirement.Permissions))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
