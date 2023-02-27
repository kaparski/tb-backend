using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net.Mail;
using System.Security.Claims;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Exceptions;
using TaxBeacon.UserManagement.Services;

namespace TaxBeacon.API.Filters;

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
            // context.Result = new UnauthorizedResult();
            return;
        }

        try
        {
            var user = await _userService.GetUserByEmailAsync(new MailAddress(email), default);

            if (user.DeactivationDateTimeUtc is not null || user.UserStatus == UserStatus.Deactivated)
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
        catch (NotFoundException ex)
        {
            context.Result = new UnauthorizedResult();
            _logger.LogError(ex, "Failed to authenticate user with {@email}", email);
        }
    }
}
