using FluentAssertions;
using FluentAssertions.Execution;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OneOf.Types;
using System.Reflection;
using System.Security.Claims;
using TaxBeacon.Administration.Departments;
using TaxBeacon.Administration.Departments.Models;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Controllers.Departments;
using TaxBeacon.API.Controllers.Departments.Requests;
using TaxBeacon.API.Controllers.Departments.Responses;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Models;

namespace TaxBeacon.API.UnitTests.Controllers.Departments;

public class DepartmentsControllerTest
{
    private readonly Mock<IDepartmentService> _serviceMock;
    private readonly DepartmentsController _controller;

    public DepartmentsControllerTest()
    {
        _serviceMock = new();

        _controller = new DepartmentsController(_serviceMock.Object)
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

    [Theory]
    [InlineData(FileType.Csv)]
    [InlineData(FileType.Xlsx)]
    public async Task ExportDepartmentsAsync_ValidQuery_ReturnsFileContent(FileType fileType)
    {
        // Arrange

        var request = new ExportDepartmentsRequest(fileType, "America/New_York");
        _serviceMock
            .Setup(x => x.ExportDepartmentsAsync(
                It.IsAny<FileType>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<byte>());

        // Act
        var actualResponse = await _controller.ExportDepartmentsAsync(request, default);

        // Assert
        using (new AssertionScope())
        {
            actualResponse.Should().NotBeNull();
            var actualResult = actualResponse as FileContentResult;
            actualResult.Should().NotBeNull();
            actualResult!.FileDownloadName.Should().Be($"departments.{fileType.ToString().ToLowerInvariant()}");
            actualResult!.ContentType.Should().Be(fileType switch
            {
                FileType.Csv => "text/csv",
                FileType.Xlsx => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                _ => throw new InvalidOperationException()
            });
        }
    }

    [Fact]
    public void Get_MarkedWithCorrectHasPermissionsAttribute()
    {
        // Arrange

        var methodInfo = ((Func<IQueryable<DepartmentResponse>>)_controller.Get).Method;
        var permissions = new object[]
        {
            Common.Permissions.Departments.Read,
            Common.Permissions.Departments.ReadWrite,
            Common.Permissions.Departments.ReadExport,
            Common.Permissions.ServiceAreas.Read,
            Common.Permissions.ServiceAreas.ReadWrite,
            Common.Permissions.ServiceAreas.ReadExport
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
    public void ExportDepartmentAsync_MarkedWithCorrectHasPermissionsAttribute()
    {
        // Arrange
        var methodInfo = ((Func<ExportDepartmentsRequest, CancellationToken, Task<IActionResult>>)_controller.ExportDepartmentsAsync).Method;

        // Act
        var hasPermissionsAttribute = methodInfo.GetCustomAttribute<HasPermissions>();

        // Assert
        using (new AssertionScope())
        {
            hasPermissionsAttribute.Should().NotBeNull();
            hasPermissionsAttribute?.Policy.Should().Be("Departments.ReadExport");
        }
    }

    [Fact]
    public async Task GetDepartmentAsync_DepartmentExists_ShouldReturnSuccessfulStatusCode()
    {
        // Arrange
        _serviceMock.Setup(x => x.GetDepartmentDetailsAsync(It.IsAny<Guid>(), default)).ReturnsAsync(new DepartmentDetailsDto());

        // Act
        var actualResponse = await _controller.GetDepartmentDetails(Guid.NewGuid(), default);

        // Assert
        using (new AssertionScope())
        {
            var actualResult = actualResponse as OkObjectResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
            actualResult?.Value.Should().BeOfType<DepartmentDetailsResponse>();
        }
    }

    [Fact]
    public async Task GetDepartmentAsync_DepartmentDoesNotExist_ShouldReturnNotFoundStatusCode()
    {
        // Arrange
        _serviceMock.Setup(x => x.GetDepartmentDetailsAsync(It.IsAny<Guid>(), default)).ReturnsAsync(new NotFound());

        // Act
        var actualResponse = await _controller.GetDepartmentDetails(Guid.NewGuid(), default);

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
    public void GetDepartmentAsync_MarkedWithCorrectHasPermissionsAttribute()
    {
        // Arrange
        var methodInfo = ((Func<Guid, CancellationToken, Task<IActionResult>>)_controller.GetDepartmentDetails).Method;

        // Act
        var hasPermissionsAttribute = methodInfo.GetCustomAttribute<HasPermissions>();

        // Assert
        using (new AssertionScope())
        {
            hasPermissionsAttribute.Should().NotBeNull();
            hasPermissionsAttribute?.Policy.Should().Be("Departments.Read;Departments.ReadWrite");
        }
    }

    [Fact]
    public async Task GetActivityHistoryAsync_DepartmentExists_ShouldReturnSuccessfulStatusCode()
    {
        // Arrange
        _serviceMock.Setup(x =>
                x.GetActivityHistoryAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>(), default))
            .ReturnsAsync(new ActivityDto(0, new ActivityItemDto[] { }));

        // Act
        var actualResponse = await _controller.ActivitiesHistory(Guid.NewGuid(), new DepartmentActivityHistoryRequest(1, 1), default);

        // Assert
        using (new AssertionScope())
        {
            var actualResult = actualResponse as OkObjectResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
            actualResult?.Value.Should().BeOfType<DepartmentActivityHistoryResponse>();
        }
    }

    [Fact]
    public async Task GetActivityHistoryAsync_DepartmentDoesNotExist_ShouldReturnNotFoundStatusCode()
    {
        // Arrange
        _serviceMock.Setup(x =>
                x.GetActivityHistoryAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>(), default))
            .ReturnsAsync(new NotFound());

        // Act
        var actualResponse =
            await _controller.ActivitiesHistory(Guid.NewGuid(), new DepartmentActivityHistoryRequest(1, 1), default);

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
    public void GetActivityHistoryAsync_MarkedWithCorrectHasPermissionsAttribute()
    {
        // Arrange
        var methodInfo = ((Func<Guid, DepartmentActivityHistoryRequest, CancellationToken, Task<IActionResult>>)_controller.ActivitiesHistory).Method;

        // Act
        var hasPermissionsAttribute = methodInfo.GetCustomAttribute<HasPermissions>();

        // Assert
        using (new AssertionScope())
        {
            hasPermissionsAttribute.Should().NotBeNull();
            hasPermissionsAttribute?.Policy.Should().Be("Departments.Read;Departments.ReadWrite");
        }
    }

    [Fact]
    public async Task GetDepartmentServiceAreasAsync_DepartmentExists_ShouldReturnSuccessfulStatusCode()
    {
        // Arrange
        _serviceMock
            .Setup(p => p.GetDepartmentServiceAreasAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync(new DepartmentServiceAreaDto[] { });

        // Act
        var actualResponse = await _controller
            .GetDepartmentServiceAreasAsync(Guid.NewGuid(), default);

        // Arrange
        using (new AssertionScope())
        {
            var actualResult = actualResponse as OkObjectResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResponse.Should().BeOfType<OkObjectResult>();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
            actualResult?.Value.Should().BeOfType<DepartmentServiceAreaResponse[]>();
        }
    }

    [Fact]
    public async Task GetDepartmentServiceAreasAsync_DepartmentDoesNotExist_ShouldReturnNotFoundStatusCode()
    {
        // Arrange
        _serviceMock
            .Setup(p => p.GetDepartmentServiceAreasAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync(new NotFound());

        // Act
        var actualResponse = await _controller
            .GetDepartmentServiceAreasAsync(Guid.NewGuid(), default);

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
    public async Task GetDepartmentJobTitlesAsync_DepartmentExists_ShouldReturnSuccessfulStatusCode()
    {
        // Arrange
        _serviceMock
            .Setup(p => p.GetDepartmentJobTitlesAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync(new DepartmentJobTitleDto[] { });

        // Act
        var actualResponse = await _controller
            .GetDepartmentJobTitlesAsync(Guid.NewGuid(), default);

        // Arrange
        using (new AssertionScope())
        {
            var actualResult = actualResponse as OkObjectResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResponse.Should().BeOfType<OkObjectResult>();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
            actualResult?.Value.Should().BeOfType<DepartmentJobTitleResponse[]>();
        }
    }

    [Fact]
    public async Task GetDepartmentJobTitlesAsync_DepartmentDoesNotExist_ShouldReturnNotFoundStatusCode()
    {
        // Arrange
        _serviceMock
            .Setup(p => p.GetDepartmentJobTitlesAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync(new NotFound());

        // Act
        var actualResponse = await _controller
            .GetDepartmentJobTitlesAsync(Guid.NewGuid(), default);

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
    public async Task UpdateDepartmentAsync_SuccessUpdate_ReturnsLocationDetailsResponse()
    {
        // Arrange
        _serviceMock
            .Setup(p => p.UpdateDepartmentAsync(It.IsAny<Guid>(),
                It.IsAny<UpdateDepartmentDto>(),
                default))
            .ReturnsAsync(new DepartmentDetailsDto());

        // Act
        var actualResponse = await _controller.UpdateDepartmentAsync(new Guid(),
            new UpdateDepartmentRequest
            {
                Name = "Tests"
            }, CancellationToken.None);

        // Arrange
        using (new AssertionScope())
        {
            var actualResult = actualResponse as OkObjectResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResponse.Should().BeOfType<OkObjectResult>();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
            actualResult?.Value.Should().BeAssignableTo<DepartmentDetailsResponse>();
        }
    }

    [Fact]
    public async Task UpdateLocationAsync_DepartmentNotExists_ReturnsNotFoundResponse()
    {
        // Arrange
        _serviceMock
            .Setup(p => p.UpdateDepartmentAsync(It.IsAny<Guid>(),
                It.IsAny<UpdateDepartmentDto>(),
                default))
            .ReturnsAsync(new NotFound());

        // Act
        var actualResponse = await _controller.UpdateDepartmentAsync(new Guid(),
            new UpdateDepartmentRequest
            {
                Name = "Tests"
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
    public void UpdateDepartmentAsync_MarkedWithCorrectHasPermissionsAttribute()
    {
        // Arrange
        var methodInfo = ((Func<Guid, UpdateDepartmentRequest, CancellationToken, Task<IActionResult>>)
            _controller.UpdateDepartmentAsync).Method;
        var permissions = new object[] { Common.Permissions.Departments.ReadWrite };

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
