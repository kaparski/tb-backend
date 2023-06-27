﻿using Microsoft.AspNetCore.Authorization;
using System.Net.Mail;
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
        var email = context.User.Claims
                        .FirstOrDefault(claim =>
                            claim.Type.Equals(Claims.EmailClaimName, StringComparison.OrdinalIgnoreCase))
                        ?.Value
                    ??
                    context.User.Claims
                        .FirstOrDefault(claim =>
                            claim.Type.Equals(Claims.OtherMails, StringComparison.OrdinalIgnoreCase))
                        ?.Value;

        if (email is null)
        {
            return;
        }

        using var scope = _scopeFactory.CreateScope();

        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
        var getUserResult = await userService.GetUserByEmailAsync(new MailAddress(email));

        if (!getUserResult.TryPickT0(out var user, out _))
        {
            context.Fail();
            _logger.LogError("Failed to authenticate user with {@email}", email);
        }

        if (user.DeactivationDateTimeUtc is not null || user.Status == Status.Deactivated)
        {
            context.Fail();
            return;
        }

        IEnumerable<string> permissions = await userService.GetUserPermissionsAsync(user.Id);

        if (permissions.Intersect(requirement.Permissions).Any())
        {
            context.Succeed(requirement);
        }
    }
}