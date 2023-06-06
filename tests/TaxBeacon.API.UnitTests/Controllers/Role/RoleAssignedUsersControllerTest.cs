using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Reflection;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Controllers.Roles;
using TaxBeacon.API.Controllers.Roles.Responses;
using TaxBeacon.UserManagement.Services;

namespace TaxBeacon.API.UnitTests.Controllers.Role;

public class RoleAssignedUsersControllerTest
{
    private readonly RoleAssignedUsersController _controller;
    private readonly Mock<IRoleService> _roleServiceMock;

    public RoleAssignedUsersControllerTest()
    {
        _roleServiceMock = new Mock<IRoleService>();

        _controller = new RoleAssignedUsersController(_roleServiceMock.Object);
    }

    [Fact]
    public void Get_MarkedWithCorrectHasPermissionsAttribute()
    {
        // Arrange
        var methodInfo = ((Func<Guid, Task<IActionResult>>)_controller.Get).Method;
        var permissions = new object[]
        {
            Common.Permissions.Roles.Read
        };

        // Act
        var hasPermissionsAttribute = methodInfo.GetCustomAttribute<HasPermissions>();

        // Assert
        using (new AssertionScope())
        {
            hasPermissionsAttribute.Should().NotBeNull();
            hasPermissionsAttribute?.Policy.Should().Be(string.Join(";", permissions.Select(x => $"{x.GetType().Name}.{x}")));
        }
    }
}
