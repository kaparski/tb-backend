using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OneOf;
using OneOf.Types;
using System.Reflection;
using System.Security.Claims;
using TaxBeacon.Accounts.Locations;
using TaxBeacon.Accounts.Locations.Models;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Controllers.Locations;
using TaxBeacon.API.Controllers.Locations.Responses;

namespace TaxBeacon.API.UnitTests.Controllers.Locations;

public class LocationsControllerTests
{
    private readonly Mock<ILocationService> _locationServiceMock;
    private readonly LocationsController _controller;

    public LocationsControllerTests()
    {
        _locationServiceMock = new Mock<ILocationService>();

        _controller = new LocationsController(_locationServiceMock.Object)
        {
            ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new[] { new ClaimsIdentity(new[] { new Claim(Claims.TenantId, Guid.NewGuid().ToString()) }) })
                }
            }
        };
    }

    [Fact]
    public void Get_HasCorrectPermissions()
    {
        // Arrange
        var methodInfo = ((Func<Guid, IActionResult>)_controller.Get).Method;
        var permissions = new object[]
        {
            Common.Permissions.Locations.Read
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

    [Fact]
    public void Get_ValidQuery_ReturnsOkStatusCode()
    {
        // Arrange
        _locationServiceMock
            .Setup(x => x.QueryLocations(It.IsAny<Guid>()))
            .Returns(OneOf<IQueryable<LocationDto>, NotFound>.FromT0(new List<LocationDto>().AsQueryable()));

        // Act
        var actualResponse = _controller.Get(Guid.NewGuid());

        // Assert
        using (new AssertionScope())
        {
            var actualResult = actualResponse as OkObjectResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
            actualResult?.Value.Should().BeAssignableTo<IQueryable<LocationResponse>>();
        }
    }

    [Fact]
    public void Get_AccountDoesNotExist_ReturnsNotFoundStatusCode()
    {
        // Arrange
        _locationServiceMock
            .Setup(x => x.QueryLocations(It.IsAny<Guid>()))
            .Returns(new NotFound());

        // Act
        var actualResponse = _controller.Get(Guid.NewGuid());

        // Assert
        using (new AssertionScope())
        {
            var actualResult = actualResponse as NotFoundResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }
    }
}
