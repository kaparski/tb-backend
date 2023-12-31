﻿using Microsoft.AspNetCore.Authentication;
using System.Net.Mail;
using System.Security.Claims;
using TaxBeacon.Administration.Users;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.API.Authentication;

public sealed class ClaimsTransformation: IClaimsTransformation
{
    private readonly IUserService _userService;

    public ClaimsTransformation(IUserService userService) => _userService = userService;

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        var email = principal.Claims
                        .SingleOrDefault(claim =>
                            claim.Type.Equals(Claims.EmailClaimName, StringComparison.OrdinalIgnoreCase))
                        ?.Value
                    ??
                    principal.Claims
                        .SingleOrDefault(claim =>
                            claim.Type.Equals(Claims.OtherMails, StringComparison.OrdinalIgnoreCase))
                        ?.Value;

        if (email == null)
        {
            return principal;
        }

        var userInfo = await _userService.GetUserInfoAsync(new MailAddress(email), default);

        if (userInfo == null || userInfo.Status == Status.Deactivated)
        {
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
