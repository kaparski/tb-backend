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
using TaxBeacon.Administration.Divisions;
using TaxBeacon.Administration.Divisions.Models;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Controllers.Divisions;
using TaxBeacon.API.Controllers.Divisions.Requests;
using TaxBeacon.API.Controllers.Divisions.Responses;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.API.UnitTests.Controllers.Divisions;

public class DivisionControllerTests
{
    private readonly Mock<IDivisionsService> _divisionsServiceMock;
    private readonly DivisionsController _controller;

    public DivisionControllerTests()
    {
        _divisionsServiceMock = new();
        _controller = new DivisionsController(_divisionsServiceMock.Object)
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
    public async Task ExportDivisionsAsync_ValidQuery_ReturnsFileContent(FileType fileType)
    {
        // Arrange
        var request = new ExportDivisionsRequest(fileType, "America/New_York");
        _divisionsServiceMock
            .Setup(x => x.ExportDivisionsAsync(
                It.IsAny<FileType>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<byte>());

        // Act
        var actualResponse = await _controller.ExportDivisionsAsync(request, default);

        // Assert
        using (new AssertionScope())
        {
            actualResponse.Should().NotBeNull();
            var actualResult = actualResponse as FileContentResult;
            actualResult.Should().NotBeNull();
            actualResult!.FileDownloadName.Should().Be($"tenantDivisions.{fileType.ToString().ToLowerInvariant()}");
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

        var methodInfo = ((Func<IQueryable<DivisionResponse>>)_controller.Get).Method;
        var permissions = new object[]
        {
            Common.Permissions.Divisions.Read,
            Common.Permissions.Divisions.ReadWrite,
            Common.Permissions.Divisions.ReadExport
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
    public void ExportDivisionsAsync_MarkedWithCorrectHasPermissionsAttribute()
    {
        // Arrange
        var methodInfo = ((Func<ExportDivisionsRequest, CancellationToken, Task<IActionResult>>)_controller.ExportDivisionsAsync).Method;

        // Act
        var hasPermissionsAttribute = methodInfo.GetCustomAttribute<HasPermissions>();

        // Assert
        using (new AssertionScope())
        {
            hasPermissionsAttribute.Should().NotBeNull();
            hasPermissionsAttribute?.Policy.Should().Be("Divisions.ReadExport");
        }
    }

    [Fact]
    public async Task GetDivisionDetailsAsync_DivisionExists_ShouldReturnSuccessfulStatusCode()
    {
        // Arrange
        _divisionsServiceMock.Setup(x => x.GetDivisionDetailsAsync(It.IsAny<Guid>(), default)).ReturnsAsync(new DivisionDetailsDto());

        // Act
        var actualResponse = await _controller.GetDivisionDetails(Guid.NewGuid(), default);

        // Assert
        using (new AssertionScope())
        {
            var actualResult = actualResponse as OkObjectResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
            actualResult?.Value.Should().BeOfType<DivisionDetailsResponse>();
        }
    }

    [Fact]
    public async Task GetDivisionDetailsAsync_DivisionDoesNotExist_ShouldReturnNotFoundStatusCode()
    {
        // Arrange
        _divisionsServiceMock.Setup(x => x.GetDivisionDetailsAsync(It.IsAny<Guid>(), default)).ReturnsAsync(new NotFound());

        // Act
        var actualResponse = await _controller.GetDivisionDetails(Guid.NewGuid(), default);

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
    public async Task GetDivisionDepartmentsAsync_DivisionExists_ShouldReturnSuccessfulStatusCode()
    {
        // Arrange
        _divisionsServiceMock
            .Setup(p => p.GetDivisionDepartmentsAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync(new DivisionDepartmentDto[] { });

        // Act
        var actualResponse = await _controller
            .GetDivisionDepartmentsAsync(Guid.NewGuid(), default);

        // Arrange
        using (new AssertionScope())
        {
            var actualResult = actualResponse as OkObjectResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResponse.Should().BeOfType<OkObjectResult>();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
            actualResult?.Value.Should().BeOfType<DivisionDepartmentResponse[]>();
        }
    }

    [Fact]
    public async Task GetDivisionDepartmentsAsync_DivisionDoesNotExist_ShouldReturnNotFoundStatusCode()
    {
        // Arrange
        _divisionsServiceMock
            .Setup(p => p.GetDivisionDepartmentsAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync(new NotFound());

        // Act
        var actualResponse = await _controller
            .GetDivisionDepartmentsAsync(Guid.NewGuid(), default);

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
    public async Task UpdateDivisionAsync_InvalidDivisionId_ReturnsNotFoundResponse()
    {
        // Arrange
        var request = TestData.UpdateDivisionFaker.Generate();
        _divisionsServiceMock
            .Setup(service => service.UpdateDivisionAsync(
                It.IsAny<Guid>(),
                It.IsAny<UpdateDivisionDto>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NotFound());

        // Act
        var actualResponse = await _controller.UpdateDivisionAsync(Guid.NewGuid(), request, default);

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
    public async Task UpdateDivisionAsync_ValidDivisionId_ReturnsUpdatedDivision()
    {
        // Arrange
        var divisionDetailsDto = TestData.DivisionDetailsFaker.Generate();
        var request = TestData.UpdateDivisionFaker.Generate();
        _divisionsServiceMock.Setup(service => service.UpdateDivisionAsync(
                It.Is<Guid>(id => id == divisionDetailsDto.Id),
                It.IsAny<UpdateDivisionDto>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(divisionDetailsDto);

        // Act
        var actualResponse = await _controller.UpdateDivisionAsync(divisionDetailsDto.Id, request, default);

        // Arrange
        using (new AssertionScope())
        {
            var actualResult = actualResponse as OkObjectResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResponse.Should().BeOfType<OkObjectResult>();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
            actualResult?.Value.Should().BeOfType<DivisionDetailsResponse>();
        }
    }

    [Fact]
    public void UpdateDivisionAsync_MarkedWithCorrectHasPermissionsAttribute()
    {
        // Arrange
        var methodInfo = ((Func<Guid, UpdateDivisionRequest, CancellationToken, Task<IActionResult>>)_controller.UpdateDivisionAsync).Method;
        var permissions = new object[] { Common.Permissions.Divisions.ReadWrite };

        // Act
        var hasPermissionsAttribute = methodInfo.GetCustomAttribute<HasPermissions>();

        // Assert
        using (new AssertionScope())
        {
            hasPermissionsAttribute.Should().NotBeNull();
            hasPermissionsAttribute?.Policy.Should().Be(string.Join(";", permissions.Select(x => $"{x.GetType().Name}.{x}")));
        }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    private static class TestData
    {
        public static readonly Faker<DivisionDto> DivisionFaker =
            new Faker<DivisionDto>()
                .RuleFor(u => u.Id, f => Guid.NewGuid())
                .RuleFor(u => u.Name, f => f.Company.CompanyName())
                .RuleFor(u => u.Description, f => f.Name.JobDescriptor())
                .RuleFor(u => u.Department, f => f.Name.JobTitle())
                .RuleFor(u => u.CreatedDateTimeUtc, f => DateTime.UtcNow)
                .RuleFor(u => u.NumberOfUsers, f => f.Random.Number(1, 10))
                .RuleFor(u => u.NumberOfDepartments, f => f.Random.Number(1, 10));

        public static readonly Faker<DivisionDetailsDto> DivisionDetailsFaker =
            new Faker<DivisionDetailsDto>()
                .RuleFor(t => t.Id, f => Guid.NewGuid())
                .RuleFor(t => t.Name, f => f.Company.CompanyName())
                .RuleFor(t => t.CreatedDateTimeUtc, f => DateTime.UtcNow);

        public static readonly Faker<UpdateDivisionRequest> UpdateDivisionFaker =
            new Faker<UpdateDivisionRequest>()
                .RuleFor(u => u.Name, f => f.Company.CompanyName())
                .RuleFor(u => u.Description, f => f.Lorem.Sentence(2));
    }
}
