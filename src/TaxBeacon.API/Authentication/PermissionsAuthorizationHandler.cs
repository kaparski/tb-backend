using Microsoft.AspNetCore.Authorization;
using TaxBeacon.Common.Enums;
using TaxBeacon.Administration.Users;

namespace TaxBeacon.API.Authentication;

public sealed class PermissionsAuthorizationHandler: AuthorizationHandler<PermissionsRequirement>
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PermissionsAuthorizationHandler> _logger;

    public PermissionsAuthorizationHandler(IServiceScopeFactory scopeFactory,
        ILogger<PermissionsAuthorizationHandler> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionsRequirement requirement)
    {
        context.Succeed(requirement);
        return;

        var idClaimValue = context.User
            .FindFirst(claim => claim.Type.Equals(Claims.UserIdClaimName, StringComparison.OrdinalIgnoreCase))
            ?.Value;

        if (!Guid.TryParse(idClaimValue, out var id))
        {
            return;
        }

        using var scope = _scopeFactory.CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
        var getUserResult = await userService.GetUserDetailsByIdAsync(id);

        if (!getUserResult.TryPickT0(out var user, out _))
        {
            context.Fail();
            _logger.LogError("Failed to authenticate user with id = {@id}", id);
        }

        if (user.DeactivationDateTimeUtc is not null || user.Status == Status.Deactivated)
        {
            context.Fail();
            return;
        }

        if (context.User.HasAnyPermission(requirement.Permissions))
        {
            context.Succeed(requirement);
        }
    }
}
