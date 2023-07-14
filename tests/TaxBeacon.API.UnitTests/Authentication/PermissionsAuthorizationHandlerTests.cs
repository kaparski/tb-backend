using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using TaxBeacon.API.Authentication;

namespace TaxBeacon.API.UnitTests.Authentication;

public sealed class PermissionsAuthorizationHandlerTests
{
    private readonly PermissionsAuthorizationHandler _permissionsAuthorizationHandler;

    public PermissionsAuthorizationHandlerTests() =>
        _permissionsAuthorizationHandler = new PermissionsAuthorizationHandler();

    [Fact]
    public async Task HandleRequirementAsync_UserHasPermission_ReturnsSuccessfulContext()
    {
        // Arrange
        var permissions = new object[]
        {
            Common.Permissions.Accounts.Read, Common.Permissions.Accounts.ReadWrite,
            Common.Permissions.Accounts.ReadExport, Common.Permissions.Clients.Read,
            Common.Permissions.Clients.ReadWrite, Common.Permissions.Clients.ReadExport,
            Common.Permissions.Referrals.Read, Common.Permissions.Referrals.ReadWrite,
            Common.Permissions.Referrals.ReadExport,
        };
        var user = new ClaimsPrincipal(
            new ClaimsIdentity(permissions.Select(p => new Claim(Claims.Permission, $"{p.GetType().Name}.{p}"))));
        var requirement =
            new PermissionsRequirement(string.Join(";", permissions.Select(p => $"{p.GetType().Name}.{p}")));
        var authContext = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await _permissionsAuthorizationHandler.HandleAsync(authContext);

        // Assert
        authContext.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_UserHasAtLeastOnePermission_ReturnsSuccessfulContext()
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(Claims.Permission,
                $"{nameof(Common.Permissions.Accounts)}.{Common.Permissions.Accounts.Read}")
        }));

        var requiredPermissions = string.Join(";",
            new object[]
            {
                Common.Permissions.Accounts.Read, Common.Permissions.Accounts.ReadWrite,
                Common.Permissions.Accounts.ReadExport, Common.Permissions.Clients.Read,
                Common.Permissions.Clients.ReadWrite, Common.Permissions.Clients.ReadExport,
                Common.Permissions.Referrals.Read, Common.Permissions.Referrals.ReadWrite,
                Common.Permissions.Referrals.ReadExport,
            }.Select(x => $"{x.GetType().Name}.{x}"));

        var requirement = new PermissionsRequirement(requiredPermissions);
        var authContext = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await _permissionsAuthorizationHandler.HandleAsync(authContext);

        // Assert
        authContext.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_UserHasNoRequiredPermissions_ReturnsFailureContext()
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity(Enumerable.Empty<Claim>()));
        var requirement =
            new PermissionsRequirement($"{nameof(Common.Permissions.Accounts)}.{Common.Permissions.Accounts.Read}");
        var authContext = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await _permissionsAuthorizationHandler.HandleAsync(authContext);

        // Assert
        authContext.HasSucceeded.Should().BeFalse();
    }
}
