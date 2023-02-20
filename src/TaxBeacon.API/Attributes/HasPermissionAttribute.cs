using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using TaxBeacon.DAL.Entities;
using TaxBeacon.UserManagement.Services;

namespace TaxBeacon.API.Attributes;

public sealed class HasPermissionAttribute: AuthorizeAttribute, IAuthorizationFilter
{
    private readonly PermissionEnum _permissionEnum;

    public HasPermissionAttribute(PermissionEnum permissionEnum) => _permissionEnum = permissionEnum;

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var userService = context.HttpContext.RequestServices.GetRequiredService<IUserService>();
        var loggingService = context.HttpContext.RequestServices.GetRequiredService<ILogger<HasPermissionAttribute>>();
    }
}
