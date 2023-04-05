using FluentAssertions;
using Gridify;
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
    private readonly Mock<ICurrentUserService> _currentServiceMock;
    private readonly Mock<IRoleService> _roleServiceMock;
    private readonly RolesController _controller;

    public RolesControllerTest()
    {
        _roleServiceMock = new();
        _currentServiceMock = new();
        _currentServiceMock
            .Setup(x => x.UserId)
            .Returns(new Guid());
        _currentServiceMock
            .Setup(x => x.TenantId)
            .Returns(new Guid());
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
    public async Task GetRoleList_ValidParameters_ReturnSuccessStatusCode()
    {
        // Arrange
        _roleServiceMock.Setup(p => p.UnassignUsersAsync(It.IsAny<Guid>(), It.IsAny<List<Guid>>(), default))
            .ReturnsAsync(new Success());

        // Act
        var actualResponse = await _controller.UnassignUsers(new Guid(), new List<Guid>(), default);

        // Assert
        actualResponse.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task GetRoleList_InvalidRole_ReturnNotFoundCode()
    {
        // Arrange
        _roleServiceMock.Setup(p => p.UnassignUsersAsync(It.IsAny<Guid>(), It.IsAny<List<Guid>>(), default))
            .ReturnsAsync(new NotFound());

        // Act
        var actualResponse = await _controller.UnassignUsers(new Guid(), new List<Guid>(), default);

        // Assert
        actualResponse.Should().BeOfType<NotFoundResult>();
    }
}
