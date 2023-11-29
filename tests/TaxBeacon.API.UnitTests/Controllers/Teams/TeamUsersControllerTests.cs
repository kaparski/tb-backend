using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Reflection;
using TaxBeacon.Administration.Teams;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Controllers.Teams;

namespace TaxBeacon.API.UnitTests.Controllers.Teams;

public class TeamUsersControllerTests
{
    private readonly Mock<ITeamService> _teamServiceMock;
    private readonly TeamUsersController _controller;

    public TeamUsersControllerTests()
    {
        _teamServiceMock = new();
        _controller = new TeamUsersController(_teamServiceMock.Object);
    }

    [Fact]
    public void Get_MarkedWithCorrectHasPermissionsAttribute()
    {
        // Arrange
        var methodInfo = ((Func<Guid, Task<IActionResult>>)_controller.Get).Method;
        var permissions = new object[]
        {
            Common.Permissions.Teams.Read,
            Common.Permissions.Teams.ReadWrite
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
