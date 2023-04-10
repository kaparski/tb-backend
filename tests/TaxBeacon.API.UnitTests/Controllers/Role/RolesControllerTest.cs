using FluentAssertions;
using FluentAssertions.Execution;
using Gridify;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OneOf.Types;
using TaxBeacon.API.Controllers.Roles;
using TaxBeacon.API.Controllers.Roles.Responses;
using TaxBeacon.Common.Services;
using TaxBeacon.UserManagement.Models;
using TaxBeacon.UserManagement.Services;

namespace TaxBeacon.API.UnitTests.Controllers.Role;

public class RolesControllerTest
{
    private readonly RolesController _controller;
    private readonly Mock<ICurrentUserService> _currentServiceMock;
    private readonly Mock<IPermissionsService> _permissionServiceMock;
    private readonly Mock<IRoleService> _roleServiceMock;

    public RolesControllerTest()
    {
        _roleServiceMock = new Mock<IRoleService>();
        _permissionServiceMock = new();
        _currentServiceMock = new Mock<ICurrentUserService>();

        _currentServiceMock
            .Setup(x => x.UserId)
            .Returns(new Guid());

        _currentServiceMock
            .Setup(x => x.TenantId)
            .Returns(new Guid());

        _controller = new RolesController(_roleServiceMock.Object, _permissionServiceMock.Object);
    }

    [Fact]
    public async Task GetRoleList_ValidQuery_ReturnSuccessStatusCode()
    {
        // Arrange
        var query = new GridifyQuery { Page = 1, PageSize = 25, OrderBy = "name asc" };

        _roleServiceMock.Setup(p => p.GetRolesAsync(query, default))
            .ReturnsAsync(new QueryablePaging<RoleDto>(0, Enumerable.Empty<RoleDto>().AsQueryable()));

        // Act
        var actualResponse = await _controller.GetRoleList(query, default);

        // Assert
        actualResponse.Should().BeOfType<ActionResult<QueryablePaging<RoleResponse>>>();
        actualResponse.Should().NotBeNull();
    }

    [Fact]
    public async Task GetRoleAssignedUsers_ValidQuery_ReturnsSuccessStatusCode()
    {
        // Arrange
        var query = new GridifyQuery { Page = 1, PageSize = 25, OrderBy = "email asc" };

        _roleServiceMock.Setup(p => p.GetRoleAssignedUsersAsync(It.IsAny<Guid>(), query, default))
            .ReturnsAsync(new QueryablePaging<UserDto>(0, Enumerable.Empty<UserDto>().AsQueryable()));

        // Act
        var actualResponse = await _controller.GetRoleAssignedUsers(It.IsAny<Guid>(), query, default);

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
        var query = new GridifyQuery { Page = 1, PageSize = 25, OrderBy = "email asc" };

        _roleServiceMock.Setup(p => p.GetRoleAssignedUsersAsync(It.IsAny<Guid>(), query, default))
            .ReturnsAsync(new NotFound());

        // Act
        var actualResponse = await _controller.GetRoleAssignedUsers(It.IsAny<Guid>(), query, default);

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
        var actualResponse = await _controller.UnassignUsers(new Guid(), new List<Guid>(), default);

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
        var actualResponse = await _controller.UnassignUsers(new Guid(), new List<Guid>(), default);

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
        var actualResponse = await _controller.AssignUsersToRole(Guid.NewGuid(), new List<Guid>(), default);

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
        var actualResponse = await _controller.AssignUsersToRole(Guid.NewGuid(), new List<Guid>(), default);

        // Assert
        using (new AssertionScope())
        {
            var actualResult = actualResponse as NotFoundResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }
    }
}
