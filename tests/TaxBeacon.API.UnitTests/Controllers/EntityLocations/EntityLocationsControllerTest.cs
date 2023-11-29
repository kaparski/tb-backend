using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OneOf.Types;
using System.Reflection;
using TaxBeacon.Accounts.EntityLocations;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Controllers.EntityLocations;
using TaxBeacon.API.Controllers.EntityLocations.Requests;
using TaxBeacon.API.FeatureManagement;
using TaxBeacon.Common.FeatureManagement;

namespace TaxBeacon.API.UnitTests.Controllers.EntityLocations;
public class EntityLocationsControllerTest
{
    private readonly Mock<IEntityLocationService> _entityLocationsServiceMock;
    private readonly EntityLocationsController _controller;

    public EntityLocationsControllerTest()
    {
        _entityLocationsServiceMock = new();
        _controller = new EntityLocationsController(_entityLocationsServiceMock.Object);
    }

    [Fact]
    public async Task AssociateEntitiesToLocationAsync_LocationAndAccountExist_ShouldReturnSuccessfulStatusCode()
    {
        // Arrange
        _entityLocationsServiceMock.Setup(x =>
                x.AssociateEntitiesToLocation(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<List<Guid>>(), default))
            .ReturnsAsync(new Success());

        // Act
        var actualResponse = await _controller.AssociateEntitiesToLocationAsync
            (Guid.NewGuid(), Guid.NewGuid(), new AssociateEntitiesToLocationRequest { EntityIds = new List<Guid>() }, default);

        // Assert
        using (new AssertionScope())
        {
            var actualResult = actualResponse as NoContentResult;
            actualResult.Should().NotBeNull();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        }
    }

    [Fact]
    public async Task AssociateEntitiesToLocationAsync_LocationNotFound_ShouldReturnNotFound()
    {
        // Arrange
        _entityLocationsServiceMock.Setup(x =>
                x.AssociateEntitiesToLocation(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<List<Guid>>(), default))
            .ReturnsAsync(new NotFound());

        // Act
        var actualResponse = await _controller.AssociateEntitiesToLocationAsync
            (Guid.NewGuid(), Guid.NewGuid(), new AssociateEntitiesToLocationRequest { EntityIds = new List<Guid>() }, default);

        // Assert
        using (new AssertionScope())
        {
            var actualResult = actualResponse as NotFoundResult;
            actualResult.Should().NotBeNull();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }
    }

    [Fact]
    public void AssociateEntitiesToLocationAsync_MarkedWithCorrectHasPermissionsAttribute()
    {
        // Arrange
        var methodInfo = ((Func<Guid, Guid, AssociateEntitiesToLocationRequest, CancellationToken, Task<IActionResult>>)
            _controller.AssociateEntitiesToLocationAsync).Method;
        var permissions = new object[] { Common.Permissions.Locations.ReadWrite };

        // Act
        var hasPermissionsAttribute = methodInfo.GetCustomAttribute<HasPermissions>();

        // Assert
        using (new AssertionScope())
        {
            hasPermissionsAttribute.Should().NotBeNull();
            hasPermissionsAttribute?.Policy.Should()
                .Be(string.Join(";", permissions.Select(x => $"{x.GetType().Name}.{x}")));
        }
    }

    [Fact]
    public async Task UnassociateLocationWithEntityAsync_ReturnSuccess()
    {
        // Arrange
        _entityLocationsServiceMock
            .Setup(p => p.UnassociateLocationWithEntityAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), default))
            .ReturnsAsync(new Success());

        // Act
        var actualResponse = await _controller.UnassociateLocationWithEntityAsync(new Guid(), new Guid());

        // Arrange
        using (new AssertionScope())
        {
            var actualResult = actualResponse as NoContentResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResponse.Should().BeOfType<NoContentResult>();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        }
    }

    [Fact]
    public async Task UnassociateLocationWithEntityAsync_LocationIdNotExistsInDb_ReturnsNotFound()
    {
        // Arrange
        _entityLocationsServiceMock
            .Setup(p => p.UnassociateLocationWithEntityAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), default))
            .ReturnsAsync(new NotFound());

        // Act
        var actualResponse = await _controller.UnassociateLocationWithEntityAsync(new Guid(), new Guid());

        // Assert
        using (new AssertionScope())
        {
            var actualResult = actualResponse as NotFoundResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }
    }

    [Fact]
    public void UnassociateLocationWithEntityAsync_MarkedWithCorrectHasPermissionsAttribute()
    {
        var methodInfo =
            ((Func<Guid, Guid, CancellationToken, Task<IActionResult>>)_controller.UnassociateLocationWithEntityAsync)
            .Method;

        var permissions = new object[] { Common.Permissions.Entities.ReadWrite, Common.Permissions.Locations.ReadWrite };

        // Act
        var hasPermissionsAttribute = methodInfo.GetCustomAttribute<HasPermissions>();

        // Assert
        using (new AssertionScope())
        {
            hasPermissionsAttribute.Should().NotBeNull();
            hasPermissionsAttribute?.Policy.Should()
                .Be(string.Join(";", permissions.Select(x => $"{x.GetType().Name}.{x}")));
        }
    }

    [Fact]
    public async Task AssociateLocationsToEntityAsync_EntityAndAccountExist_ShouldReturnSuccessfulStatusCode()
    {
        // Arrange
        _entityLocationsServiceMock.Setup(x =>
                x.AssociateLocationsToEntity(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<List<Guid>>(), default))
            .ReturnsAsync(new Success());

        // Act
        var actualResponse = await _controller.AssociateLocationsToEntityAsync
            (Guid.NewGuid(), Guid.NewGuid(), new AssociateLocationsToEntityRequest { LocationIds = new List<Guid>() }, default);

        // Assert
        using (new AssertionScope())
        {
            var actualResult = actualResponse as NoContentResult;
            actualResult.Should().NotBeNull();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        }
    }

    [Fact]
    public async Task AssociateLocationsToEntityAsync_LocationNotFound_ShouldReturnNotFound()
    {
        // Arrange
        _entityLocationsServiceMock.Setup(x =>
                x.AssociateLocationsToEntity(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<List<Guid>>(), default))
            .ReturnsAsync(new NotFound());

        // Act
        var actualResponse = await _controller.AssociateLocationsToEntityAsync
            (Guid.NewGuid(), Guid.NewGuid(), new AssociateLocationsToEntityRequest { LocationIds = new List<Guid>() }, default);

        // Assert
        using (new AssertionScope())
        {
            var actualResult = actualResponse as NotFoundResult;
            actualResult.Should().NotBeNull();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }
    }

    [Fact]
    public void AssociateLocationsToEntityAsync_MarkedWithCorrectHasPermissionsAttribute()
    {
        // Arrange
        var methodInfo = ((Func<Guid, Guid, AssociateLocationsToEntityRequest, CancellationToken, Task<IActionResult>>)
            _controller.AssociateLocationsToEntityAsync).Method;
        var permissions = new object[] { Common.Permissions.Entities.ReadWrite };

        // Act
        var hasPermissionsAttribute = methodInfo.GetCustomAttribute<HasPermissions>();

        // Assert
        using (new AssertionScope())
        {
            hasPermissionsAttribute.Should().NotBeNull();
            hasPermissionsAttribute?.Policy.Should()
                .Be(string.Join(";", permissions.Select(x => $"{x.GetType().Name}.{x}")));
        }
    }
}
