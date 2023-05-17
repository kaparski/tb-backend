using FluentAssertions;
using FluentAssertions.Execution;
using Gridify;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OneOf.Types;
using System.Reflection;
using System.Security.Claims;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Controllers.Programs;
using TaxBeacon.API.Controllers.Programs.Requests;
using TaxBeacon.API.Controllers.Programs.Response;
using TaxBeacon.Common.Enums;
using TaxBeacon.UserManagement.Models;
using TaxBeacon.UserManagement.Models.Programs;
using TaxBeacon.UserManagement.Services;

namespace TaxBeacon.API.UnitTests.Controllers.Programs;

public class ProgramsControllerTests
{
    private readonly Mock<IProgramService> _programServiceMock;
    private readonly ProgramsController _controller;

    public ProgramsControllerTests()
    {
        _programServiceMock = new();
        _controller = new ProgramsController(_programServiceMock.Object)
        {
            ControllerContext = new ControllerContext
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
    public async Task GetAllProgramsAsync_ValidQuery_ReturnsSuccessfulStatusCode()
    {
        // Arrange
        var query = new GridifyQuery { Page = 1, PageSize = 25, OrderBy = "name desc", };
        _programServiceMock.Setup(p => p.GetAllProgramsAsync(query, default))
            .ReturnsAsync(
                new QueryablePaging<ProgramDto>(0, Enumerable.Empty<ProgramDto>().AsQueryable()));

        // Act
        var actualResponse = await _controller.GetAllProgramsAsync(query, default);

        // Arrange
        using (new AssertionScope())
        {
            var actualResult = actualResponse as OkObjectResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResponse.Should().BeOfType<OkObjectResult>();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
            actualResult?.Value.Should().BeOfType<QueryablePaging<ProgramResponse>>();
        }
    }

    [Fact]
    public async Task GetAllProgramsAsync_InvalidQuery_ReturnsBadRequest()
    {
        // Arrange
        var query = new GridifyQuery { Page = 1, PageSize = 25, OrderBy = "nonexistentfield desc", };
        _programServiceMock.Setup(p => p.GetAllProgramsAsync(query, default))
            .ReturnsAsync(
                new QueryablePaging<ProgramDto>(0, Enumerable.Empty<ProgramDto>().AsQueryable()));

        // Act
        var actualResponse = await _controller.GetAllProgramsAsync(query, default);

        // Arrange
        using (new AssertionScope())
        {
            var actualResult = actualResponse as BadRequestResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResponse.Should().BeOfType<BadRequestResult>();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        }
    }

    [Fact]
    public void GetAllProgramsAsync_HasAppropriatePermissions()
    {
        // Arrange
        var methodInfo = ((Func<GridifyQuery, CancellationToken, Task<IActionResult>>)_controller.GetAllProgramsAsync).Method;
        var permissions = new object[]
        {
            Common.Permissions.Programs.Read,
            Common.Permissions.Programs.ReadWrite,
            Common.Permissions.Programs.ReadExport
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

    [Theory]
    [InlineData(FileType.Csv)]
    [InlineData(FileType.Xlsx)]
    public async Task ExportProgramsAsync_ValidQuery_ReturnsFileContent(FileType fileType)
    {
        // Arrange
        var request = new ExportProgramsRequest(fileType, "America/New_York");
        _programServiceMock
            .Setup(x => x.ExportProgramsAsync(
                It.IsAny<FileType>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<byte>());

        // Act
        var actualResponse = await _controller.ExportProgramsAsync(request, default);

        // Assert
        using (new AssertionScope())
        {
            actualResponse.Should().NotBeNull();
            var actualResult = actualResponse as FileContentResult;
            actualResult.Should().NotBeNull();
            actualResult!.FileDownloadName.Should().Be($"programs.{fileType.ToString().ToLowerInvariant()}");
            actualResult!.ContentType.Should().Be(fileType switch
            {
                FileType.Csv => "text/csv",
                FileType.Xlsx => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                _ => throw new InvalidOperationException()
            });
        }
    }

    [Fact]
    public void ExportProgramsAsync_HasAppropriatePermissions()
    {
        // Arrange
        var methodInfo = ((Func<ExportProgramsRequest, CancellationToken, Task<IActionResult>>)_controller.ExportProgramsAsync).Method;
        var permissions = new object[]
        {
            Common.Permissions.Programs.ReadExport
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
    public async Task GetProgramDetailsAsync_ProgramExists_ReturnsSuccessfulStatusCode()
    {
        // Arrange
        _programServiceMock
            .Setup(x => x.GetProgramDetailsAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync(new ProgramDetailsDto());

        // Act
        var actualResponse = await _controller.GetProgramDetailsAsync(Guid.NewGuid(), default);

        // Assert
        using (new AssertionScope())
        {
            var actualResult = actualResponse as OkObjectResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
            actualResult?.Value.Should().BeOfType<ProgramDetailsResponse>();
        }
    }

    [Fact]
    public async Task GetProgramDetailsAsync_ProgramDoesNotExist_ReturnsNotFoundStatusCode()
    {
        // Arrange
        _programServiceMock
            .Setup(x => x.GetProgramDetailsAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync(new NotFound());

        // Act
        var actualResponse = await _controller.GetProgramDetailsAsync(Guid.NewGuid(), default);

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
    public void GetProgramDetailsAsync_HasAppropriatePermissions()
    {
        // Arrange
        var methodInfo = ((Func<Guid, CancellationToken, Task<IActionResult>>)_controller.GetProgramDetailsAsync).Method;
        var permissions = new object[]
        {
            Common.Permissions.Programs.Read,
            Common.Permissions.Programs.ReadWrite,
            Common.Permissions.Programs.ReadExport
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
    public async Task GetProgramActivityHistoryAsync_ProgramExists_ReturnsSuccessfulStatusCode()
    {
        // Arrange
        _programServiceMock.Setup(x =>
                x.GetProgramActivityHistory(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>(), default))
            .ReturnsAsync(new ActivityDto(0, new ActivityItemDto[] { }));

        // Act
        var actualResponse = await _controller.GetProgramActivityHistoryAsync(
            Guid.NewGuid(), new ProgramActivityHistoryRequest(1, 1), default);

        // Assert
        using (new AssertionScope())
        {
            var actualResult = actualResponse as OkObjectResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
            actualResult?.Value.Should().BeOfType<ProgramActivityHistoryResponse>();
        }
    }

    [Fact]
    public async Task GetProgramActivityHistoryAsync_ProgramDoesNotExist_ReturnsNotFoundStatusCode()
    {
        // Arrange
        _programServiceMock.Setup(x =>
                x.GetProgramActivityHistory(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>(), default))
            .ReturnsAsync(new NotFound());

        // Act
        var actualResponse = await _controller.GetProgramActivityHistoryAsync(
            Guid.NewGuid(), new ProgramActivityHistoryRequest(1, 1), default);

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
    public void GetProgramActivityHistoryAsync_HasAppropriatePermissions()
    {
        // Arrange
        var methodInfo = ((Func<Guid, ProgramActivityHistoryRequest, CancellationToken, Task<IActionResult>>)_controller.GetProgramActivityHistoryAsync).Method;
        var permissions = new object[]
        {
            Common.Permissions.Programs.Read,
            Common.Permissions.Programs.ReadWrite,
            Common.Permissions.Programs.ReadExport
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
