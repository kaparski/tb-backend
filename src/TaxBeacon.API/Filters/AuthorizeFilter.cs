using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TaxBeacon.API.Authentication;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.API.Filters;

public class AuthorizeFilter: IAsyncAuthorizationFilter
{
    private readonly ILogger<AuthorizeFilter> _logger;

    public AuthorizeFilter(ILogger<AuthorizeFilter> logger) => _logger = logger;

    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

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
