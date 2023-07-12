using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net.Mail;
using System.Security.Claims;
using TaxBeacon.Common.Enums;
using TaxBeacon.Administration.Users;

namespace TaxBeacon.API.Filters;

[Obsolete("Not used")]
public class AuthorizeFilter: IAsyncAuthorizationFilter
{
    private const string UserIdClaimName = "userId";
    private const string EmailClaimName = "preferred_username";
    private readonly ILogger<AuthorizeFilter> _logger;
    private readonly IUserService _userService;

    public AuthorizeFilter(ILogger<AuthorizeFilter> logger, IUserService userService)
    {
        _logger = logger;
        _userService = userService;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var email = context.HttpContext.User.Claims
            .FirstOrDefault(claim => claim.Type.Equals(EmailClaimName, StringComparison.OrdinalIgnoreCase))
            ?.Value;

        if (email is null)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var getUserResult = await _userService.GetUserByEmailAsync(new MailAddress(email));

        if (!getUserResult.TryPickT0(out var user, out _))
        {
            context.Result = new UnauthorizedResult();
            _logger.LogError("Failed to authenticate user with {@email}", email);
            return;
        }

        if (user.DeactivationDateTimeUtc is not null || user.Status == Status.Deactivated)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        if (!context.HttpContext.User.HasClaim(claim =>
                claim.Type.Equals(UserIdClaimName, StringComparison.OrdinalIgnoreCase)))
        {
            var claimsIdentity = new ClaimsIdentity();
            claimsIdentity.AddClaim(new Claim(UserIdClaimName, user.Id.ToString()));
            context.HttpContext.User.AddIdentity(claimsIdentity);
        }
    }
}
