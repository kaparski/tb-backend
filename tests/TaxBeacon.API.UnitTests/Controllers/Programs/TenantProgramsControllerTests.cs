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

namespace TaxBeacon.API.UnitTests.Controllers.Programs;

public class TenantProgramsControllerTests
{
    private readonly Mock<IProgramService> _programServiceMock;
    private readonly TenantProgramsController _controller;

    public static readonly Guid TenantId = Guid.NewGuid();

    public TenantProgramsControllerTests()
    {
        _programServiceMock = new();
        _controller = new TenantProgramsController(_programServiceMock.Object)
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
        var methodInfo = ((Func<IQueryable<TenantProgramResponse>>)_controller.Get)
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
    [InlineData(Status.Deactivated)]
    [InlineData(Status.Active)]
    public async Task UpdateTenantProgramStatusAsync_NewProgramStatus_ReturnsUpdatedTenantProgram(Status status)
    {
        // Arrange
        var tenantProgramDto = TestData.TenantProgramFaker.Generate();
        tenantProgramDto.Status = Status.Deactivated;
        tenantProgramDto.DeactivationDateTimeUtc = DateTime.UtcNow;

        _programServiceMock
            .Setup(service => service.UpdateTenantProgramStatusAsync(
                It.Is<Guid>(id => id == tenantProgramDto.Id),
                It.IsAny<Status>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenantProgramDto);

        // Act
        var actualResponse = await _controller.UpdateProgramStatusAsync(tenantProgramDto.Id, status, default);

        // Arrange
        using (new AssertionScope())
        {
            var actualResult = actualResponse.Result as OkObjectResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResponse.Should().BeOfType<ActionResult<TenantProgramDetailsResponse>>();
            actualResponse.Result.Should().BeOfType<OkObjectResult>();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
            actualResult?.Value.Should().BeOfType<TenantProgramDetailsResponse>();
            (actualResult?.Value as TenantProgramDetailsResponse)?.Id.Should().Be(tenantProgramDto.Id);
        }
    }

    [Theory]
    [InlineData(FileType.Csv)]
    [InlineData(FileType.Xlsx)]
    public async Task ExportTenantProgramsAsync_ValidQuery_ReturnsFileContent(FileType fileType)
    {
        // Arrange
        var request = new ExportProgramsRequest(fileType, "America/New_York");
        _programServiceMock
            .Setup(x => x.ExportTenantProgramsAsync(
                It.IsAny<FileType>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<byte>());

        // Act
        var actualResponse = await _controller.ExportTenantProgramsAsync(request, default);

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
    public void ExportTenantProgramsAsync_HasAppropriatePermissions()
    {
        // Arrange
        var methodInfo =
            ((Func<ExportProgramsRequest, CancellationToken, Task<IActionResult>>)_controller.ExportTenantProgramsAsync)
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
    public async Task GetTenantProgramDetailsAsync_ProgramExists_ReturnsSuccessfulStatusCode()
    {
        // Arrange
        _programServiceMock
            .Setup(x => x.GetTenantProgramDetailsAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync(new TenantProgramDetailsDto());

        // Act
        var actualResponse = await _controller.GetTenantProgramDetailsAsync(Guid.NewGuid(), default);

        // Assert
        using (new AssertionScope())
        {
            var actualResult = actualResponse as OkObjectResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
            actualResult?.Value.Should().BeOfType<TenantProgramDetailsResponse>();
        }
    }

    [Fact]
    public async Task GetTenantProgramDetailsAsync_ProgramDoesNotExist_ReturnsNotFoundStatusCode()
    {
        // Arrange
        _programServiceMock
            .Setup(x => x.GetTenantProgramDetailsAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync(new NotFound());

        // Act
        var actualResponse = await _controller.GetTenantProgramDetailsAsync(Guid.NewGuid(), default);

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
    public void GetTenantProgramDetailsAsync_HasAppropriatePermissions()
    {
        // Arrange
        var methodInfo = ((Func<Guid, CancellationToken, Task<IActionResult>>)_controller.GetTenantProgramDetailsAsync)
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
