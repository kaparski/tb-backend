using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Reflection;
using TaxBeacon.Administration.JobTitles;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Controllers.JobTitles;

namespace TaxBeacon.API.UnitTests.Controllers.JobTitles;

public class JobTitleUsersControllerTests
{
    private readonly Mock<IJobTitleService> _jobTitleServiceMock;
    private readonly JobTitleUsersController _controller;

    public JobTitleUsersControllerTests()
    {
        _jobTitleServiceMock = new();

        _controller = new JobTitleUsersController(_jobTitleServiceMock.Object);
    }

    [Fact]
    public void Get_MarkedWithCorrectHasPermissionsAttribute()
    {
        // Arrange
        var methodInfo = ((Func<Guid, Task<IActionResult>>)_controller.Get).Method;
        var permissions = new object[]
        {
            Common.Permissions.JobTitles.Read,
            Common.Permissions.JobTitles.ReadWrite
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
