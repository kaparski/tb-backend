using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TaxBeacon.DAL.Entities;
using TaxBeacon.UserManagement.Services;

namespace TaxBeacon.API.Attributes;

public sealed class HasPermissionAttribute: AuthorizeAttribute, IAsyncAuthorizationFilter
{
    private readonly PermissionEnum _permission;
    private const string EmailClaimName = "userId";

    public HasPermissionAttribute(PermissionEnum permission) => _permission = permission;

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var email = context.HttpContext.User.Claims
            .FirstOrDefault(claim => claim.Type.Equals(EmailClaimName, StringComparison.OrdinalIgnoreCase))
            ?.Value;

        if (email is null)
        {
            return;
        }

        var userService = context.HttpContext.RequestServices.GetRequiredService<IUserService>();
        var logger =
            context.HttpContext.RequestServices.GetRequiredService<ILogger<HasPermissionAttribute>>();
        try
        {
            var permissions = await userService.GetUserPermissionsByEmailAsync(email);

            if (!permissions.Contains(_permission))
            {
                context.Result = new ForbidResult();
            }
        }
        catch (Exception ex)
        {
            context.Result = new ForbidResult();
            logger.LogError(ex, "Failed to authenticate user with {@email}", email);
        }
    }
}
