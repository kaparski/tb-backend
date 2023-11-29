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
using TaxBeacon.API.Controllers.Locations.Requests;
using TaxBeacon.API.Controllers.Locations.Responses;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Enums.Accounts;
using TaxBeacon.Common.Models;

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
                    User = new ClaimsPrincipal(new[]
                    {
                        new ClaimsIdentity(new[]
                        {
                            new Claim(Claims.TenantId, Guid.NewGuid().ToString())
                        })
                    })
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
            Common.Permissions.Locations.Read,
            Common.Permissions.Locations.ReadActivation,
            Common.Permissions.Locations.ReadWrite,
            Common.Permissions.Locations.ReadExport,
        };

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

    [Fact]
    public async Task GetLocationDetailsAsync_LocationExists_ReturnsSuccessfulStatusCode()
    {
        // Arrange
        _locationServiceMock
            .Setup(x => x.GetLocationDetailsAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), default))
            .ReturnsAsync(new LocationDetailsDto());

        // Act
        var actualResponse = await _controller.GetLocationDetailsAsync(Guid.NewGuid(), Guid.NewGuid(), default);

        // Assert
        using (new AssertionScope())
        {
            var actualResult = actualResponse as OkObjectResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
            actualResult?.Value.Should().BeOfType<LocationDetailsResponse>();
        }
    }

    [Fact]
    public async Task GetLocationDetailsAsync_LocationDoesNotExist_ReturnsNotFoundStatusCode()
    {
        // Arrange
        _locationServiceMock
            .Setup(x => x.GetLocationDetailsAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), default))
            .ReturnsAsync(new NotFound());

        // Act
        var actualResponse = await _controller.GetLocationDetailsAsync(Guid.NewGuid(), It.IsAny<Guid>(), default);

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
    public void GetLocationDetailsAsync_MarkedWithCorrectHasPermissionsAttribute()
    {
        // Arrange
        var methodInfo =
            ((Func<Guid, Guid, CancellationToken, Task<IActionResult>>)_controller.GetLocationDetailsAsync)
            .Method;

        var permissions = new object[]
        {
            Common.Permissions.Locations.Read, Common.Permissions.Locations.ReadActivation,
            Common.Permissions.Locations.ReadWrite
        };

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
    public async Task UpdateLocationStatusAsync_ExistingLocationId_ReturnSuccessStatusCode()
    {
        // Arrange
        _locationServiceMock.Setup(p =>
                p.UpdateLocationStatusAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Status>(), default))
            .ReturnsAsync(
                new LocationDetailsDto());

        // Act
        var actualResponse =
            await _controller.UpdateLocationStatusAsync(new Guid(), new Guid(), Status.Active, default);

        // Arrange
        using (new AssertionScope())
        {
            var actualResult = actualResponse as OkObjectResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResponse.Should().BeOfType<OkObjectResult>();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
            actualResult?.Value.Should().BeAssignableTo<LocationDetailsResponse>();
        }
    }

    [Fact]
    public async Task UpdateLocationStatusAsync_NonExistingLocationId_ReturnNotFoundStatusCode()
    {
        // Arrange
        _locationServiceMock.Setup(p =>
                p.UpdateLocationStatusAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Status>(), default))
            .ReturnsAsync(new NotFound());

        // Act
        var actualResponse =
            await _controller.UpdateLocationStatusAsync(new Guid(), new Guid(), Status.Active, default);

        // Arrange
        using (new AssertionScope())
        {
            var actualResult = actualResponse as NotFoundResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }
    }

    [Fact]
    public void UpdateLocationStatusAsync_MarkedWithCorrectHasPermissionsAttribute()
    {
        // Arrange
        var methodInfo =
            ((Func<Guid, Guid, Status, CancellationToken, Task<IActionResult>>)_controller.UpdateLocationStatusAsync)
            .Method;
        var permissions = new object[] { Common.Permissions.Locations.ReadActivation };

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
    public async Task CreateNewLocationAsync_ValidRequest_ReturnsLocationDetailsResponse()
    {
        // Arrange
        _locationServiceMock
            .Setup(s => s.CreateNewLocationAsync(It.IsAny<Guid>(),
                It.IsAny<CreateLocationDto>(),
                default))
            .ReturnsAsync(new LocationDetailsDto());

        // Act
        var actualResponse = await _controller.CreateNewLocationAsync(new Guid(),
            new CreateLocationRequest
            {
                Country = Country.UnitedStates,
                Type = LocationType.Headquarters,
                Name = "Tests",
                LocationId = "Test"
            },
            default);

        // Arrange
        using (new AssertionScope())
        {
            var actualResult = actualResponse as OkObjectResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResponse.Should().BeOfType<OkObjectResult>();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
            actualResult?.Value.Should().BeAssignableTo<LocationDetailsResponse>();
        }
    }

    [Fact]
    public async Task CreateNewLocationAsync_AccountNotExists_ReturnsNotFoundResponse()
    {
        // Arrange
        _locationServiceMock
            .Setup(s => s.CreateNewLocationAsync(It.IsAny<Guid>(),
                It.IsAny<CreateLocationDto>(),
                default))
            .ReturnsAsync(new NotFound());

        // Act
        var actualResponse = await _controller.CreateNewLocationAsync(new Guid(),
            new CreateLocationRequest
            {
                Country = Country.UnitedStates,
                Type = LocationType.Headquarters,
                Name = "Tests",
                LocationId = "Test"
            },
            default);

        // Arrange
        using (new AssertionScope())
        {
            var actualResult = actualResponse as NotFoundResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }
    }

    [Fact]
    public void CreateNewLocationAsync_MarkedWithCorrectHasPermissionsAttribute()
    {
        // Arrange
        var methodInfo = ((Func<Guid, CreateLocationRequest, CancellationToken, Task<IActionResult>>)
            _controller.CreateNewLocationAsync).Method;
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
    public async Task UpdateLocationAsync_NoConflictErrors_ReturnsLocationDetailsResponse()
    {
        // Arrange
        _locationServiceMock
            .Setup(p => p.UpdateLocationAsync(It.IsAny<Guid>(), It.IsAny<Guid>(),
                It.IsAny<UpdateLocationDto>(),
                default))
            .ReturnsAsync(new LocationDetailsDto());

        // Act
        var actualResponse = await _controller.UpdateLocationAsync(new Guid(), new Guid(),
            new UpdateLocationRequest
            {
                Country = Country.UnitedStates,
                Type = LocationType.Headquarters,
                Name = "Tests",
                LocationId = "Test"
            }, CancellationToken.None);

        // Arrange
        using (new AssertionScope())
        {
            var actualResult = actualResponse as OkObjectResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResponse.Should().BeOfType<OkObjectResult>();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
            actualResult?.Value.Should().BeAssignableTo<LocationDetailsResponse>();
        }
    }

    [Fact]
    public async Task UpdateLocationAsync_LocationNotExists_ReturnsNotFoundResponse()
    {
        // Arrange
        _locationServiceMock
            .Setup(p => p.UpdateLocationAsync(It.IsAny<Guid>(), It.IsAny<Guid>(),
                It.IsAny<UpdateLocationDto>(),
                default))
            .ReturnsAsync(new NotFound());

        // Act
        var actualResponse = await _controller.UpdateLocationAsync(new Guid(), new Guid(),
            new UpdateLocationRequest
            {
                Country = Country.UnitedStates,
                Type = LocationType.Headquarters,
                Name = "Tests",
                LocationId = "Test"
            }, CancellationToken.None);

        // Arrange
        using (new AssertionScope())
        {
            var actualResult = actualResponse as NotFoundResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }
    }

    [Fact]
    public void UpdateLocationAsync_MarkedWithCorrectHasPermissionsAttribute()
    {
        // Arrange
        var methodInfo = ((Func<Guid, Guid, UpdateLocationRequest, CancellationToken, Task<IActionResult>>)
            _controller.UpdateLocationAsync).Method;
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
    public async Task LocationActivitiesHistory_LocationExists_ShouldReturnSuccessfulStatusCode()
    {
        // Arrange
        _locationServiceMock.Setup(x =>
                x.GetActivitiesAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>(), default))
            .ReturnsAsync(new ActivityDto(0, new ActivityItemDto[] { }));

        // Act
        var actualResponse = await _controller.LocationActivitiesHistory
            (Guid.NewGuid(), Guid.NewGuid(), new LocationActivityRequest(1, 1), default);

        // Assert
        using (new AssertionScope())
        {
            var actualResult = actualResponse as OkObjectResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
            actualResult?.Value.Should().BeOfType<LocationActivityResponse>();
        }
    }

    [Fact]
    public async Task LocationActivitiesHistory_LocationDoesNotExist_ShouldReturnNotFoundStatusCode()
    {
        // Arrange
        _locationServiceMock.Setup(x =>
                x.GetActivitiesAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>(), default))
            .ReturnsAsync(new NotFound());

        // Act
        var actualResponse =
            await _controller.LocationActivitiesHistory(Guid.NewGuid(), Guid.NewGuid(),
                new LocationActivityRequest(1, 1), default);

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
    public void LocationActivitiesHistory_MarkedWithCorrectHasPermissionsAttribute()
    {
        // Arrange
        var methodInfo =
            ((Func<Guid, Guid, LocationActivityRequest, CancellationToken, Task<IActionResult>>)_controller
                .LocationActivitiesHistory).Method;

        // Act
        var hasPermissionsAttribute = methodInfo.GetCustomAttribute<HasPermissions>();

        // Assert
        using (new AssertionScope())
        {
            hasPermissionsAttribute.Should().NotBeNull();
            hasPermissionsAttribute?.Policy.Should().Be("Locations.Read;Locations.ReadWrite");
        }
    }

    [Theory]
    [InlineData(FileType.Csv)]
    [InlineData(FileType.Xlsx)]
    public async Task ExportLocationsAsync_ValidQuery_ReturnsFileContent(FileType fileType)
    {
        // Arrange
        var request = new ExportLocationsRequest(fileType, "America/New_York");
        _locationServiceMock
            .Setup(x => x.ExportLocationsAsync(
                It.IsAny<Guid>(),
                It.IsAny<FileType>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<byte>());

        // Act
        var actualResponse = await _controller.ExportLocationsAsync(Guid.NewGuid(), request, default);

        // Assert
        using (new AssertionScope())
        {
            actualResponse.Should().NotBeNull();
            var actualResult = actualResponse as FileContentResult;
            actualResult.Should().NotBeNull();
            actualResult!.FileDownloadName.Should().Be($"locations.{fileType.ToString().ToLowerInvariant()}");
            actualResult.ContentType.Should().Be(fileType switch
            {
                FileType.Csv => "text/csv",
                FileType.Xlsx => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                _ => throw new InvalidOperationException()
            });
        }
    }

    [Theory]
    [InlineData(FileType.Csv)]
    [InlineData(FileType.Xlsx)]
    public async Task ExportLocationsAsync_AccountDoesNotExist_ReturnsNotFound(FileType fileType)
    {
        // Arrange
        var request = new ExportLocationsRequest(fileType, "America/New_York");
        _locationServiceMock
            .Setup(x => x.ExportLocationsAsync(
                It.IsAny<Guid>(),
                It.IsAny<FileType>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NotFound());

        // Act
        var actualResponse = await _controller.ExportLocationsAsync(Guid.NewGuid(), request, default);

        // Assert
        using (new AssertionScope())
        {
            actualResponse.Should().NotBeNull();
            var actualResult = actualResponse as NotFoundResult;
            actualResult.Should().NotBeNull();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }
    }

    [Fact]
    public void ExportLocationsAsync_MarkedWithCorrectHasPermissionsAttribute()
    {
        // Arrange
        var methodInfo = ((Func<Guid, ExportLocationsRequest, CancellationToken, Task<IActionResult>>)
            _controller.ExportLocationsAsync).Method;
        var permissions = new object[] { Common.Permissions.Locations.ReadExport };

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
    public void GenerateLocationIdAsync_MarkedWithCorrectHasPermissionsAttribute()
    {
        // Arrange
        var methodInfo = ((Func<CancellationToken, Task<IActionResult>>)
            _controller.GenerateLocationIdAsync).Method;
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
}
