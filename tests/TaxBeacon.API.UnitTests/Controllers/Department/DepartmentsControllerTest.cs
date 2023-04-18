using FluentAssertions;
using FluentAssertions.Execution;
using Gridify;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Reflection;
using System.Security.Claims;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Controllers.Departments;
using TaxBeacon.API.Controllers.Departments.Requests;
using TaxBeacon.API.Controllers.Departments.Responses;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Services;
using TaxBeacon.UserManagement.Models;
using TaxBeacon.UserManagement.Services;

namespace TaxBeacon.API.UnitTests.Controllers.Tenant;

public class DepartmentsControllerTest
{
    private readonly Mock<ITenantService> _tenantServiceMock;
    private readonly Mock<ICurrentUserService> _currentServiceMock;
    private readonly DepartmentsController _controller;

    public DepartmentsControllerTest()
    {
        _tenantServiceMock = new();
        _currentServiceMock = new();

        _controller = new DepartmentsController(_tenantServiceMock.Object, _currentServiceMock.Object)
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
    public async Task GetDepartmentList_ValidQuery_ReturnSuccessStatusCode()
    {
        // Arrange

        Guid tenantId = new();
        _currentServiceMock
            .Setup(x => x.TenantId)
            .Returns(tenantId);

        var query = new GridifyQuery { Page = 1, PageSize = 25, OrderBy = "name desc", };
        _tenantServiceMock.Setup(p => p.GetDepartmentsAsync(tenantId, query, default)).ReturnsAsync(
            new QueryablePaging<DepartmentDto>(0,
                Enumerable.Empty<DepartmentDto>().AsQueryable()));

        // Act
        var actualResponse = await _controller.GetDepartmentList(query, default);

        // Arrange
        using (new AssertionScope())
        {
            var actualResult = actualResponse as OkObjectResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResponse.Should().BeOfType<OkObjectResult>();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
            actualResult?.Value.Should().BeOfType<QueryablePaging<DepartmentResponse>>();
        }
    }

    [Fact]
    public async Task GetDepartmentList_InvalidQuery_ReturnBadRequest()
    {
        // Arrange
        var query = new GridifyQuery { Page = 1, PageSize = 25, OrderBy = "nonexistentfield desc", };

        // Act
        var actualResponse = await _controller.GetDepartmentList(query, default);

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

    [Theory]
    [InlineData(FileType.Csv)]
    [InlineData(FileType.Xlsx)]
    public async Task ExportDepartmentsAsync_ValidQuery_ReturnsFileContent(FileType fileType)
    {
        // Arrange

        Guid tenantId = new();
        _currentServiceMock
            .Setup(x => x.TenantId)
            .Returns(tenantId);

        var request = new ExportDepartmentsRequest(fileType, "America/New_York");
        _tenantServiceMock
            .Setup(x => x.ExportDepartmentsAsync(
                tenantId,
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
    public void GetDepartmentList_MarkedWithCorrectHasPermissionsAttribute()
    {
        // Arrange
        var methodInfo = ((Func<GridifyQuery, CancellationToken, Task<IActionResult>>)_controller.GetDepartmentList).Method;

        // Act
        var hasPermissionsAttribute = methodInfo.GetCustomAttribute<HasPermissions>();

        // Assert
        using (new AssertionScope())
        {
            hasPermissionsAttribute.Should().NotBeNull();
            hasPermissionsAttribute?.Policy.Should().Be("Departments.Read;Departments.ReadWrite;Departments.ReadExport");
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
}
