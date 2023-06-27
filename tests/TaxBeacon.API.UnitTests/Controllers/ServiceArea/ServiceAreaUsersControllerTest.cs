using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Reflection;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Controllers.ServiceAreas;
using TaxBeacon.UserManagement.ServiceAreas;

namespace TaxBeacon.API.UnitTests.Controllers.ServiceArea;

public class ServiceAreaUsersControllerTest
{
    private readonly Mock<IServiceAreaService> _serviceAreaServiceMock;
    private readonly ServiceAreaUsersController _controller;

    public ServiceAreaUsersControllerTest()
    {
        _serviceAreaServiceMock = new();

        _controller = new ServiceAreaUsersController(_serviceAreaServiceMock.Object);
    }

    [Fact]
    public void Get_MarkedWithCorrectHasPermissionsAttribute()
    {
        // Arrange
        var methodInfo = ((Func<Guid, Task<IActionResult>>)_controller.Get).Method;
        var permissions = new object[]
        {
            Common.Permissions.ServiceAreas.Read,
            Common.Permissions.ServiceAreas.ReadWrite
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
