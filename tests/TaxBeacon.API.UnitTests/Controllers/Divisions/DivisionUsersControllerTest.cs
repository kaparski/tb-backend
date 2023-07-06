using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Reflection;
using TaxBeacon.Administration.Divisions;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Controllers.Divisions;

namespace TaxBeacon.API.UnitTests.Controllers.Divisions;

public class DivisionUsersControllerTest
{
    private readonly Mock<IDivisionsService> _divisionsServiceMock;
    private readonly DivisionUsersController _controller;

    public DivisionUsersControllerTest()
    {
        _divisionsServiceMock = new();
        _controller = new DivisionUsersController(_divisionsServiceMock.Object);
    }

    [Fact]
    public void Get_MarkedWithCorrectHasPermissionsAttribute()
    {
        // Arrange
        var methodInfo = ((Func<Guid, Task<IActionResult>>)_controller.Get).Method;
        var permissions = new object[]
        {
            Common.Permissions.Divisions.Read,
            Common.Permissions.Divisions.ReadWrite
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