using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OneOf.Types;
using TaxBeacon.API.Controllers.Role;
using TaxBeacon.Common.Services;
using TaxBeacon.UserManagement.Services;

namespace TaxBeacon.API.UnitTests.Controllers.Role;

public class RoleControllerTest
{
    private readonly Mock<ICurrentUserService> _currentServiceMock;
    private readonly Mock<IRoleService> _roleServiceMock;
    private readonly RoleController _controller;

    public RoleControllerTest()
    {
        _roleServiceMock = new();
        _currentServiceMock = new();
        _currentServiceMock
            .Setup(x => x.UserId)
            .Returns(new Guid());
        _currentServiceMock
            .Setup(x => x.TenantId)
            .Returns(new Guid());
        _controller = new RoleController(_roleServiceMock.Object, _currentServiceMock.Object);
    }

    [Fact]
    public async Task GetRoleList_ValidQuery_ReturnSuccessStatusCode()
    {
        // Arrange
        _roleServiceMock.Setup(p => p.UnassignUsersAsync(It.IsAny<List<Guid>>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), default))
            .ReturnsAsync(new Success());

        // Act
        var actualResponse = await _controller.UnassignUsers(new Guid(), new List<Guid>(), default);

        // Assert
        actualResponse.Should().BeOfType<OkResult>();
        actualResponse.Should().NotBeNull();
    }
}
