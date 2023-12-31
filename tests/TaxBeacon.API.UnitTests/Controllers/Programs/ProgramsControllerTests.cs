using Bogus;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OneOf.Types;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Security.Claims;
using TaxBeacon.Administration.Programs;
using TaxBeacon.Administration.Programs.Models;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Controllers.Programs.Requests;
using TaxBeacon.API.Controllers.Programs;
using TaxBeacon.API.Controllers.Programs.Responses;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Enums.Administration;
using TaxBeacon.Common.Errors;
using TaxBeacon.Common.Models;

namespace TaxBeacon.API.UnitTests.Controllers.Programs;

public class ProgramsControllerTests
{
    private readonly Mock<IProgramService> _programServiceMock;
    private readonly ProgramsController _controller;

    public static readonly Guid TenantId = Guid.NewGuid();

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
    public void Get_HasAppropriatePermissions()
    {
        // Arrange
        var methodInfo = ((Func<IQueryable<ProgramResponse>>)_controller.Get)
            .Method;
        var permissions = new object[]
        {
            Common.Permissions.Programs.Read, Common.Permissions.Programs.ReadWrite,
            Common.Permissions.Programs.ReadExport
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
        var methodInfo =
            ((Func<ExportProgramsRequest, CancellationToken, Task<IActionResult>>)_controller.ExportProgramsAsync)
            .Method;
        var permissions = new object[] { Common.Permissions.Programs.ReadExport };

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
    public async Task GetProgramActivityHistoryAsync_ProgramExists_ReturnsSuccessfulStatusCode()
    {
        // Arrange
        _programServiceMock.Setup(x =>
                x.GetProgramActivityHistoryAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>(), default))
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
                x.GetProgramActivityHistoryAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>(), default))
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
    public void GetProgramDetailsAsync_HasAppropriatePermissions()
    {
        // Arrange
        var methodInfo = ((Func<Guid, CancellationToken, Task<IActionResult>>)_controller.GetProgramDetailsAsync)
            .Method;
        var permissions = new object[]
        {
            Common.Permissions.Programs.Read, Common.Permissions.Programs.ReadWrite,
            Common.Permissions.Programs.ReadExport
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
    public void GetProgramActivityHistoryAsync_HasAppropriatePermissions()
    {
        // Arrange
        var methodInfo =
            ((Func<Guid, ProgramActivityHistoryRequest, CancellationToken, Task<IActionResult>>)_controller
                .GetProgramActivityHistoryAsync).Method;
        var permissions = new object[]
        {
            Common.Permissions.Programs.Read, Common.Permissions.Programs.ReadWrite,
            Common.Permissions.Programs.ReadExport
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
    public async Task UpdateProgramAsync_ProgramExistsAndRequestIsValid_ShouldReturnSuccessfulStatusCode()
    {
        // Arrange
        var request = TestData.UpdateProgramRequestFaker.Generate();
        var program = TestData.ProgramDetailsDtoFaker.Generate();
        _programServiceMock.Setup(x =>
                x.UpdateProgramAsync(It.Is<Guid>(id => id == program.Id), It.IsAny<UpdateProgramDto>(), default))
            .ReturnsAsync(program);

        // Act
        var actualResponse = await _controller.UpdateProgramAsync(program.Id, request, default);

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
    public async Task UpdateProgramAsync_ProgramDoesNotExists_ShouldReturnNotFoundStatusCode()
    {
        // Arrange
        var request = TestData.UpdateProgramRequestFaker.Generate();
        _programServiceMock.Setup(x => x.UpdateProgramAsync(It.IsAny<Guid>(), It.IsAny<UpdateProgramDto>(), default))
            .ReturnsAsync(new NotFound());

        // Act
        var actualResponse = await _controller.UpdateProgramAsync(Guid.NewGuid(), request, default);

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
    public async Task UpdateProgramAsync_ProgramWithNameAlreadyExists_ShouldReturnConflictStatusCode()
    {
        // Arrange
        var request = TestData.UpdateProgramRequestFaker.Generate();
        _programServiceMock.Setup(x => x.UpdateProgramAsync(It.IsAny<Guid>(), It.IsAny<UpdateProgramDto>(), default))
            .ReturnsAsync(new NameAlreadyExists());

        // Act
        var actualResponse = await _controller.UpdateProgramAsync(Guid.NewGuid(), request, default);

        // Assert
        using (new AssertionScope())
        {
            var actualResult = actualResponse as ConflictResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status409Conflict);
        }
    }

    [Fact]
    public void UpdateProgramAsync_MarkedWithCorrectHasPermissionsAttribute()
    {
        // Arrange
        var methodInfo =
            ((Func<Guid, UpdateProgramRequest, CancellationToken, Task<IActionResult>>)_controller.UpdateProgramAsync)
            .Method;
        var permissions = new object[] { Common.Permissions.Programs.ReadWrite };

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
    public async Task CreateProgramAsync_ProgramDoesNotExists_ShouldReturnSuccessfulStatusCode()
    {
        // Arrange
        var request = TestData.CreateProgramRequestFaker.Generate();
        var program = TestData.ProgramDetailsDtoFaker.Generate();
        _programServiceMock.Setup(x => x.CreateProgramAsync(It.IsAny<CreateProgramDto>(), default))
            .ReturnsAsync(program);

        // Act
        var actualResponse = await _controller.CreateProgramAsync(request, default);

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
    public async Task CreateProgramAsync_ProgramWithNameAlreadyExists_ShouldReturnConflictStatusCode()
    {
        // Arrange
        var request = TestData.CreateProgramRequestFaker.Generate();
        _programServiceMock.Setup(x => x.CreateProgramAsync(It.IsAny<CreateProgramDto>(), default))
            .ReturnsAsync(new NameAlreadyExists());

        // Act
        var actualResponse = await _controller.CreateProgramAsync(request, default);

        // Assert
        using (new AssertionScope())
        {
            var actualResult = actualResponse as ConflictResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status409Conflict);
        }
    }

    [Fact]
    public void CreateProgramAsync_MarkedWithCorrectHasPermissionsAttribute()
    {
        // Arrange
        var methodInfo =
            ((Func<CreateProgramRequest, CancellationToken, Task<IActionResult>>)_controller.CreateProgramAsync)
            .Method;
        var permissions = new object[] { Common.Permissions.Programs.ReadWrite };

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

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    private static class TestData
    {
        public static readonly Faker<ProgramDetailsDto> ProgramDetailsDtoFaker =
            new Faker<ProgramDetailsDto>()
                .RuleFor(t => t.Id, _ => Guid.NewGuid())
                .RuleFor(t => t.Name, f => f.Company.CompanyName())
                .RuleFor(t => t.CreatedDateTimeUtc, _ => DateTime.UtcNow);

        public static readonly Faker<UpdateProgramRequest> UpdateProgramRequestFaker =
            new Faker<UpdateProgramRequest>().CustomInstantiator(f => new UpdateProgramRequest(
                f.Lorem.Word(),
                f.Company.CompanyName(),
                f.Lorem.Text(),
                f.Lorem.Word(),
                f.Internet.Url(),
                f.PickRandom<Jurisdiction>(),
                f.Address.State(),
                f.Address.Country(),
                f.Address.City(),
                f.Lorem.Word(),
                f.Lorem.Word(),
                f.Date.Past(),
                f.Date.Future()));

        public static readonly Faker<CreateProgramRequest> CreateProgramRequestFaker =
            new Faker<CreateProgramRequest>().CustomInstantiator(f => new CreateProgramRequest(
                f.Lorem.Word(),
                f.Company.CompanyName(),
                f.Lorem.Text(),
                f.Lorem.Word(),
                f.Internet.Url(),
                f.PickRandom<Jurisdiction>(),
                f.Address.State(),
                f.Address.Country(),
                f.Address.City(),
                f.Lorem.Word(),
                f.Lorem.Word(),
                f.Date.Past(),
                f.Date.Future()));

        public static readonly Faker<TenantProgramDetailsDto> TenantProgramFaker = new Faker<TenantProgramDetailsDto>()
            .RuleFor(p => p.Status, f => f.PickRandom<Status>())
            .RuleFor(u => u.ReactivationDateTimeUtc, _ => DateTime.UtcNow)
            .RuleFor(u => u.DeactivationDateTimeUtc, _ => DateTime.UtcNow);
    }
}
