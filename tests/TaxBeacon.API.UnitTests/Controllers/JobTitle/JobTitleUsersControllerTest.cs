using Bogus;
using FluentAssertions;
using FluentAssertions.Execution;
using Gridify;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OneOf.Types;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Security.Claims;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Controllers.JobTitles;
using TaxBeacon.API.Controllers.JobTitles.Requests;
using TaxBeacon.API.Controllers.JobTitles.Responses;
using TaxBeacon.API.Controllers.Tenants.Responses;
using TaxBeacon.Common.Enums;
using TaxBeacon.UserManagement.Models;
using TaxBeacon.UserManagement.Services;

namespace TaxBeacon.API.UnitTests.Controllers.JobTitle;

public class JobTitleUsersControllerTest
{
    private readonly Mock<IJobTitleService> _jobTitleServiceMock;
    private readonly JobTitleUsersController _controller;

    public JobTitleUsersControllerTest()
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
