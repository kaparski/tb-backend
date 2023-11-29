using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TaxBeacon.API.Authentication;

namespace TaxBeacon.API.Filters;

public class AuthorizeFilter: IAsyncAuthorizationFilter
{
    private readonly ILogger<AuthorizeFilter> _logger;

    public AuthorizeFilter(ILogger<AuthorizeFilter> logger) => _logger = logger;

    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var isRequiredAuthorization = context.ActionDescriptor.EndpointMetadata
            .Any(em => em.GetType() == typeof(AuthorizeAttribute));

        var allowAnonymous = context.ActionDescriptor.EndpointMetadata
            .Any(em => em.GetType() == typeof(AllowAnonymousAttribute));

        if (allowAnonymous || !isRequiredAuthorization)
        {
            return Task.CompletedTask;
        }

        var idClaimValue = context.HttpContext.User
            .FindFirst(claim => claim.Type.Equals(Claims.UserId, StringComparison.OrdinalIgnoreCase))
            ?.Value;

        if (string.IsNullOrEmpty(idClaimValue))
        {
            context.Result = new UnauthorizedResult();
            _logger.LogError("Failed to authenticate a user");
        }

        return Task.CompletedTask;
    }
}
