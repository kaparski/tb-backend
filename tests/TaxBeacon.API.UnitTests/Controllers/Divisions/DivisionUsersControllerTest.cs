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
using TaxBeacon.API.Controllers.Departments.Responses;
using TaxBeacon.API.Controllers.Tenants;
using TaxBeacon.API.Controllers.Tenants.Requests;
using TaxBeacon.API.Controllers.Tenants.Responses;
using TaxBeacon.Common.Enums;
using TaxBeacon.UserManagement.Models;
using TaxBeacon.UserManagement.Services;

namespace TaxBeacon.API.UnitTests.Controllers.Divisions
{
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
}
