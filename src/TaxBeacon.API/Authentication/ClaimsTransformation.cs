using Microsoft.AspNetCore.Authentication;
using System.Net.Mail;
using System.Security.Claims;
using TaxBeacon.Administration.Users;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.API.Authentication;

public sealed class ClaimsTransformation: IClaimsTransformation
{
    private readonly IUserService _userService;
    private readonly ILogger<ClaimsTransformation> _logger;

    public ClaimsTransformation(IUserService userService, ILogger<ClaimsTransformation> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        var idpExternalId = principal.Claims
            .Single(claim => claim.Type.Equals(ClaimTypes.NameIdentifier, StringComparison.OrdinalIgnoreCase))
            .Value;

        var email = principal.GetEmail();

        if (email == null)
        {
            return principal;
        }

        var userInfo = await _userService.GetUserInfoAsync(new MailAddress(email), default);

        if (userInfo == null || userInfo.Status == Status.Deactivated)
        {
            return principal;
        }

        // setup external id at the first login to app
        if (userInfo.IdpExternalId is null)
        {
            await _userService.SetIdpExternalIdAsync(new MailAddress(email), idpExternalId);
            userInfo = userInfo with { IdpExternalId = idpExternalId };
        }

        if (userInfo.IdpExternalId != idpExternalId)
        {
            _logger.LogCritical("Attempt to get access with email {email} and external id {ActualExternalId} with different external id {ProvidedExternalId}",
                email,
                userInfo.IdpExternalId,
                idpExternalId);
            return principal;
        }

        var claimsIdentity = new ClaimsIdentity();

        if (!principal.HasClaim(claim => claim.Type == Claims.UserId))
        {
            claimsIdentity.AddClaim(new Claim(Claims.UserId, userInfo.Id.ToString()));
        }

        if (!principal.HasClaim(claim => claim.Type == Claims.TenantId))
        {
            claimsIdentity.AddClaim(new Claim(Claims.TenantId, userInfo.TenantId.ToString()));
        }

        if (!principal.HasClaim(claim => claim.Type == Claims.Roles))
        {
            claimsIdentity.AddClaims(userInfo.Roles.Select(r => new Claim(Claims.Roles, r)));
        }

        if (!principal.HasClaim(claim => claim.Type == Claims.TenantRoles))
        {
            claimsIdentity.AddClaims(userInfo.TenantRoles.Select(r => new Claim(Claims.TenantRoles, r)));
        }

        if (!principal.HasClaim(claim => claim.Type == Claims.FullName))
        {
            claimsIdentity.AddClaim(new Claim(Claims.FullName, userInfo.FullName));
        }

        if (!principal.HasClaim(claim => claim.Type == Claims.DivisionEnabled))
        {
            claimsIdentity.AddClaim(new Claim(Claims.DivisionEnabled, userInfo.DivisionEnabled.ToString()));
        }

        var permissions = await _userService.GetUserPermissionsAsync(userInfo.Id, userInfo.TenantId);
        claimsIdentity.AddClaims(permissions.Select(p => new Claim(Claims.Permission, p)));

        principal.AddIdentity(claimsIdentity);
        return principal;
    }
}
