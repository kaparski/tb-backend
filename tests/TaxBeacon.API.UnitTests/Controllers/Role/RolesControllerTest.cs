using FluentAssertions;
using FluentAssertions.Execution;
using Gridify;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TaxBeacon.API.Controllers.Roles;
using TaxBeacon.API.Controllers.Roles.Responses;
using TaxBeacon.UserManagement.Models;
using TaxBeacon.UserManagement.Services;

namespace TaxBeacon.API.UnitTests.Controllers.Role;

public class RolesControllerTest
{
    private readonly Mock<IRoleService> _roleServiceMock;
    private readonly RolesController _controller;

    public RolesControllerTest()
    {
        _roleServiceMock = new();

        _controller = new RolesController(_roleServiceMock.Object);
    }

    [Fact]
    public async Task GetRoleList_ValidQuery_ReturnSuccessStatusCode()
    {
        // Arrange
        var query = new GridifyQuery { Page = 1, PageSize = 25, OrderBy = "name asc", };
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
        var query = new GridifyQuery { Page = 1, PageSize = 25, OrderBy = "email asc", };
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
        var query = new GridifyQuery { Page = 1, PageSize = 25, OrderBy = "email asc", };
        _roleServiceMock.Setup(p => p.GetRoleAssignedUsersAsync(It.IsAny<Guid>(), query, default))
            .ReturnsAsync(new OneOf.Types.NotFound());

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
}
