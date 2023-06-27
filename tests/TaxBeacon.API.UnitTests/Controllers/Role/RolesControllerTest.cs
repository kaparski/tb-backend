using FluentAssertions;
using FluentAssertions.Execution;
using Gridify;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OneOf.Types;
using System.Reflection;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Controllers.JobTitles.Responses;
using TaxBeacon.API.Controllers.Roles;
using TaxBeacon.API.Controllers.Roles.Responses;
using TaxBeacon.UserManagement.Roles;
using TaxBeacon.UserManagement.Roles.Models;

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
    public async Task GetRoleList_ValidQuery_ReturnSuccessStatusCode()
    {
        // Arrange
        var query = new GridifyQuery
        {
            Page = 1,
            PageSize = 25,
            OrderBy = "name asc"
        };

        _roleServiceMock.Setup(p => p.GetRolesAsync(query, default))
            .ReturnsAsync(new QueryablePaging<RoleDto>(0, Enumerable.Empty<RoleDto>().AsQueryable()));

        // Act
        var actualResponse = await _controller.GetRoleList(query);

        // Assert
        actualResponse.Should().BeOfType<ActionResult<QueryablePaging<RoleResponse>>>();
        actualResponse.Should().NotBeNull();
    }

    [Fact]
    public async Task GetRoleAssignedUsers_ValidQuery_ReturnsSuccessStatusCode()
    {
        // Arrange
        var query = new GridifyQuery
        {
            Page = 1,
            PageSize = 25,
            OrderBy = "email asc"
        };

        _roleServiceMock.Setup(p => p.GetRoleAssignedUsersAsync(It.IsAny<Guid>(), query, default))
            .ReturnsAsync(
                new QueryablePaging<RoleAssignedUserDto>(0, Enumerable.Empty<RoleAssignedUserDto>().AsQueryable()));

        // Act
        var actualResponse = await _controller.GetRoleAssignedUsers(It.IsAny<Guid>(), query);

        // Assert
        using (new AssertionScope())
        {
            var actualResult = actualResponse as OkObjectResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
            actualResult?.Value.Should().BeOfType<QueryablePaging<RoleAssignedUserResponse>>();
        }
    }

    [Fact]
    public async Task GetRoleAssignedUsers_RoleDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var query = new GridifyQuery
        {
            Page = 1,
            PageSize = 25,
            OrderBy = "email asc"
        };

        _roleServiceMock.Setup(p => p.GetRoleAssignedUsersAsync(It.IsAny<Guid>(), query, default))
            .ReturnsAsync(new NotFound());

        // Act
        var actualResponse = await _controller.GetRoleAssignedUsers(It.IsAny<Guid>(), query);

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
