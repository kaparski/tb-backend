using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OneOf.Types;
using System.Reflection;
using TaxBeacon.Administration.Roles;
using TaxBeacon.Administration.Roles.Models;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Controllers.Roles;
using TaxBeacon.API.Controllers.Roles.Responses;

namespace TaxBeacon.API.UnitTests.Controllers.Role;

public class RolesControllerTest
{
    private readonly RolesController _controller;
    private readonly Mock<IRoleService> _roleServiceMock;

    public RolesControllerTest()
    {
        _roleServiceMock = new Mock<IRoleService>();

        _controller = new RolesController(_roleServiceMock.Object);
    }

    [Fact]
    public async Task UnassignUsersAsync_ValidUserIds_ReturnSuccessStatusCode()
    {
        // Arrange
        _roleServiceMock.Setup(p => p.UnassignUsersAsync(It.IsAny<Guid>(), It.IsAny<List<Guid>>(), default))
            .ReturnsAsync(new Success());

        // Act
        var actualResponse = await _controller.UnassignUsers(new Guid(), new List<Guid>());

        // Assert
        actualResponse.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task UnassignUsersAsync_InvalidRole_ReturnNotFoundCode()
    {
        // Arrange
        _roleServiceMock.Setup(p => p.UnassignUsersAsync(It.IsAny<Guid>(), It.IsAny<List<Guid>>(), default))
            .ReturnsAsync(new NotFound());

        // Act
        var actualResponse = await _controller.UnassignUsers(new Guid(), new List<Guid>());

        // Assert
        actualResponse.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task AssignUsersToRole_ExistingRoleId_ReturnSuccessfulStatusCode()
    {
        // Arrange
        _roleServiceMock.Setup(p => p.AssignUsersAsync(It.IsAny<Guid>(), It.IsAny<List<Guid>>(), default))
            .ReturnsAsync(new Success());

        // Act
        var actualResponse = await _controller.AssignUsersToRole(Guid.NewGuid(), new List<Guid>());

        // Assert
        using (new AssertionScope())
        {
            var actualResult = actualResponse as OkResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
        }
    }

    [Fact]
    public async Task AssignUsersToRole_NonExistingRoleId_ReturnNotFoundStatusCode()
    {
        // Arrange
        _roleServiceMock.Setup(p => p.AssignUsersAsync(It.IsAny<Guid>(), It.IsAny<List<Guid>>(), default))
            .ReturnsAsync(new NotFound());

        // Act
        var actualResponse = await _controller.AssignUsersToRole(Guid.NewGuid(), new List<Guid>());

        // Assert
        using (new AssertionScope())
        {
            var actualResult = actualResponse as NotFoundResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }
    }

    [Fact]
    public async Task GetPermissionsByRoleId_ExistingRoleId_ReturnSuccessfulStatusCode()
    {
        // Arrange
        _roleServiceMock
            .Setup(p => p.GetRolePermissionsByIdAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync(Array.Empty<PermissionDto>());

        // Act
        var actualResponse = await _controller.GetPermissionsByRoleId(Guid.NewGuid());

        // Assert
        using (new AssertionScope())
        {
            var actualResult = actualResponse as OkObjectResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
            actualResult?.Value.Should().BeOfType<List<PermissionResponse>>();
        }
    }

    [Fact]
    public async Task GetPermissionsByRoleId_NonExistingRoleId_ReturnNotFoundStatusCode()
    {
        // Arrange
        _roleServiceMock
            .Setup(p => p.GetRolePermissionsByIdAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync(new NotFound());

        // Act
        var actualResponse = await _controller.GetPermissionsByRoleId(Guid.NewGuid());

        // Assert
        using (new AssertionScope())
        {
            var actualResult = actualResponse as NotFoundResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }
    }

    [Fact]
    public void Get_MarkedWithCorrectHasPermissionsAttribute()
    {
        // Arrange
        var methodInfo = ((Func<IQueryable<RoleResponse>>)_controller.Get).Method;
        var permissions = new object[]
        {
            Common.Permissions.Roles.Read,
            Common.Permissions.Roles.ReadWrite
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
