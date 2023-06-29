using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Reflection;
using TaxBeacon.Administration.Departments;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Controllers.Departments;

namespace TaxBeacon.API.UnitTests.Controllers.Department;

public class DepartmentUsersControllerTest
{
    private readonly Mock<IDepartmentService> _serviceMock;
    private readonly DepartmentUsersController _controller;

    public DepartmentUsersControllerTest()
    {
        _serviceMock = new();

        _controller = new DepartmentUsersController(_serviceMock.Object);
    }

    [Fact]
    public void Get_MarkedWithCorrectHasPermissionsAttribute()
    {
        // Arrange
        var methodInfo = ((Func<Guid, Task<IActionResult>>)_controller.Get).Method;
        var permissions = new object[]
        {
            Common.Permissions.Departments.Read,
            Common.Permissions.Departments.ReadWrite
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
