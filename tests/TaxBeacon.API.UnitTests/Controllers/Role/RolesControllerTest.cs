using FluentAssertions;
using Gridify;
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
        _roleServiceMock.Setup(p => p.GetRolesAsync(It.IsAny<Guid>(), query, default))
            .ReturnsAsync(new QueryablePaging<RoleDto>(0, Enumerable.Empty<RoleDto>().AsQueryable()));

        // Act
        var actualResponse = await _controller.GetRoleList(query, default);

        // Assert
        actualResponse.Should().BeOfType<ActionResult<QueryablePaging<RoleResponse>>>();
        actualResponse.Should().NotBeNull();
    }

    [Fact]
    public async Task GetRoleUsers_ValidQuery_ReturnsSuccessStatusCode()
    {
        // Arrange
        var query = new GridifyQuery { Page = 1, PageSize = 25, OrderBy = "email asc", };
        _roleServiceMock.Setup(p => p.GetRoleUsersAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), query, default))
            .ReturnsAsync(new QueryablePaging<UserDto>(0, Enumerable.Empty<UserDto>().AsQueryable()));

        // Act
        var actualResponse = await _controller.GetRoleUsers(It.IsAny<Guid>(), query, default);

        // Assert
        actualResponse.Should().BeOfType<ActionResult<QueryablePaging<RoleUserResponse>>>();
        actualResponse.Should().NotBeNull();
    }
}
