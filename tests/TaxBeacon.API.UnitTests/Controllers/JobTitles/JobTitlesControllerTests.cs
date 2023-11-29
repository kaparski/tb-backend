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
using TaxBeacon.Administration.JobTitles;
using TaxBeacon.Administration.JobTitles.Models;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Controllers.JobTitles;
using TaxBeacon.API.Controllers.JobTitles.Requests;
using TaxBeacon.API.Controllers.JobTitles.Responses;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Models;

namespace TaxBeacon.API.UnitTests.Controllers.JobTitles;

public class JobTitlesControllerTests
{
    private readonly Mock<IJobTitleService> _jobTitleServiceMock;
    private readonly JobTitlesController _controller;
    private readonly Guid _tenantId = Guid.NewGuid();

    public JobTitlesControllerTests()
    {
        _jobTitleServiceMock = new();

        _controller = new JobTitlesController(_jobTitleServiceMock.Object)
        {
            ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new[] { new ClaimsIdentity(new[] { new Claim(Claims.TenantId, _tenantId.ToString()) }) })
                }
            }
        };
    }

    [Theory]
    [InlineData(FileType.Csv)]
    [InlineData(FileType.Xlsx)]
    public async Task ExportJobTitlesAsync_ValidQuery_ReturnsFileContent(FileType fileType)
    {
        // Arrange
        var request = new ExportJobTitlesRequest(fileType, "America/New_York");
        _jobTitleServiceMock
            .Setup(x => x.ExportJobTitlesAsync(
                It.IsAny<FileType>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<byte>());

        // Act
        var actualResponse = await _controller.ExportJobTitlesAsync(request, default);

        // Assert
        using (new AssertionScope())
        {
            actualResponse.Should().NotBeNull();
            var actualResult = actualResponse as FileContentResult;
            actualResult.Should().NotBeNull();
            actualResult!.FileDownloadName.Should().Be($"jobtitles.{fileType.ToString().ToLowerInvariant()}");
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
        var methodInfo = ((Func<IQueryable<JobTitleResponse>>)_controller.Get).Method;
        var permissions = new object[]
        {
            Common.Permissions.Departments.Read,
            Common.Permissions.Departments.ReadWrite,
            Common.Permissions.Departments.ReadExport,
            Common.Permissions.JobTitles.Read,
            Common.Permissions.JobTitles.ReadWrite,
            Common.Permissions.JobTitles.ReadExport
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
    public void ExportJobTitlesAsync_MarkedWithCorrectHasPermissionsAttribute()
    {
        // Arrange
        var methodInfo = ((Func<ExportJobTitlesRequest, CancellationToken, Task<IActionResult>>)_controller.ExportJobTitlesAsync).Method;
        var permissions = new object[]
        {
            Common.Permissions.JobTitles.ReadExport
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
    public async Task GetJobTitleAsync_JobTitleExists_ShouldReturnSuccessfulStatusCode()
    {
        // Arrange
        _jobTitleServiceMock.Setup(x => x.GetJobTitleDetailsByIdAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync(new JobTitleDetailsDto());

        // Act
        var actualResponse = await _controller.GetJobTitleAsync(Guid.NewGuid(), default);

        // Assert
        using (new AssertionScope())
        {
            var actualResult = actualResponse as OkObjectResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
            actualResult?.Value.Should().BeOfType<JobTitleDetailsResponse>();
        }
    }

    [Fact]
    public async Task GetJobTitleAsync_JobTitleDoesNotExist_ShouldReturnNotFoundStatusCode()
    {
        // Arrange
        _jobTitleServiceMock.Setup(x => x.GetJobTitleDetailsByIdAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync(new NotFound());

        // Act
        var actualResponse = await _controller.GetJobTitleAsync(Guid.NewGuid(), default);

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
    public void GetJobTitleAsync_MarkedWithCorrectHasPermissionsAttribute()
    {
        // Arrange
        var methodInfo = ((Func<Guid, CancellationToken, Task<IActionResult>>)_controller.GetJobTitleAsync).Method;
        var permissions = new object[] { Common.Permissions.JobTitles.Read, Common.Permissions.JobTitles.ReadWrite };

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
    public async Task GetActivityHistoryAsync_TenantExists_ShouldReturnSuccessfulStatusCode()
    {
        // Arrange
        _jobTitleServiceMock.Setup(x =>
                x.GetActivityHistoryAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>(), default))
            .ReturnsAsync(new ActivityDto(0, new ActivityItemDto[] { }));

        // Act
        var actualResponse = await _controller.GetActivityHistoryAsync(Guid.NewGuid(), new JobTitleActivityHistoryRequest(1, 1), default);

        // Assert
        using (new AssertionScope())
        {
            var actualResult = actualResponse as OkObjectResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
            actualResult?.Value.Should().BeOfType<JobTitleActivityHistoryResponse>();
        }
    }

    [Fact]
    public async Task GetActivityHistoryAsync_TenantDoesNotExist_ShouldReturnNotFoundStatusCode()
    {
        // Arrange
        _jobTitleServiceMock.Setup(x =>
                x.GetActivityHistoryAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>(), default))
            .ReturnsAsync(new NotFound());

        // Act
        var actualResponse =
            await _controller.GetActivityHistoryAsync(Guid.NewGuid(), new JobTitleActivityHistoryRequest(1, 1), default);

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
        var methodInfo = ((Func<Guid, JobTitleActivityHistoryRequest, CancellationToken, Task<IActionResult>>)_controller.GetActivityHistoryAsync).Method;
        var permissions = new object[] { Common.Permissions.JobTitles.Read, Common.Permissions.JobTitles.ReadWrite };

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
    public async Task UpdateJobTitleAsync_JobTitleExistsAndRequestIsValid_ShouldReturnSuccessfulStatusCode()
    {
        // Arrange
        var request = TestData.TestUpdateJobTitleRequest.Generate();
        var jobTitle = TestData.TestJobTitleDetailsDto.Generate();
        _jobTitleServiceMock.Setup(x => x.UpdateJobTitleDetailsAsync(It.Is<Guid>(id => id == jobTitle.Id), It.IsAny<UpdateJobTitleDto>(), default))
            .ReturnsAsync(jobTitle);

        // Act
        var actualResponse = await _controller.UpdateJobTitleAsync(jobTitle.Id, request, default);

        // Assert
        using (new AssertionScope())
        {
            var actualResult = actualResponse as OkObjectResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
            actualResult?.Value.Should().BeOfType<JobTitleDetailsResponse>();
        }
    }

    [Fact]
    public async Task UpdateJobTitleAsync_JobTitleDoesNotExists_ShouldReturnNotFoundStatusCode()
    {
        // Arrange
        var request = TestData.TestUpdateJobTitleRequest.Generate();
        _jobTitleServiceMock.Setup(x => x.UpdateJobTitleDetailsAsync(It.IsAny<Guid>(), It.IsAny<UpdateJobTitleDto>(), default))
            .ReturnsAsync(new NotFound());

        // Act
        var actualResponse = await _controller.UpdateJobTitleAsync(Guid.NewGuid(), request, default);

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
    public void UpdateJobTitleAsync_MarkedWithCorrectHasPermissionsAttribute()
    {
        // Arrange
        var methodInfo = ((Func<Guid, UpdateJobTitleRequest, CancellationToken, Task<IActionResult>>)_controller.UpdateJobTitleAsync).Method;
        var permissions = new object[] { Common.Permissions.JobTitles.ReadWrite };

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
        public static readonly Faker<JobTitleDetailsDto> TestJobTitleDetailsDto =
            new Faker<JobTitleDetailsDto>()
                .RuleFor(t => t.Id, f => Guid.NewGuid())
                .RuleFor(t => t.Name, f => f.Company.CompanyName())
                .RuleFor(t => t.CreatedDateTimeUtc, f => DateTime.UtcNow);

        public static readonly Faker<UpdateJobTitleRequest> TestUpdateJobTitleRequest =
            new Faker<UpdateJobTitleRequest>()
                .RuleFor(t => t.Name, f => f.Company.CompanyName())
                .RuleFor(t => t.Description, f => f.Lorem.Sentence(2));
    }
}
