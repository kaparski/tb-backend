using FluentAssertions;
using FluentAssertions.Execution;
using Gridify;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Reflection;
using System.Security.Claims;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Controllers.Tenants;
using TaxBeacon.API.Controllers.Tenants.Responses;
using TaxBeacon.API.Controllers.Users.Requests;
using TaxBeacon.Common.Enums;
using TaxBeacon.UserManagement.Models;
using TaxBeacon.UserManagement.Services;

namespace TaxBeacon.API.UnitTests.Controllers.Tenant;

public class TenantsControllerTest
{
    private readonly Mock<ITenantService> _tenantServiceMock;
    private readonly TenantsController _controller;

    public TenantsControllerTest()
    {
        _tenantServiceMock = new();
        _controller = new TenantsController(_tenantServiceMock.Object)
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
    public async Task GetTenantList_ValidQuery_ReturnSuccessStatusCode()
    {
        // Arrange
        var query = new GridifyQuery { Page = 1, PageSize = 25, OrderBy = "name desc", };
        _tenantServiceMock.Setup(p => p.GetTenantsAsync(query, default)).ReturnsAsync(
            new QueryablePaging<TenantDto>(0,
                Enumerable.Empty<TenantDto>().AsQueryable()));

        // Act
        var actualResponse = await _controller.GetTenantList(query, default);

        // Arrange
        using (new AssertionScope())
        {
            var actualResult = actualResponse as OkObjectResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResponse.Should().BeOfType<OkObjectResult>();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
            actualResult?.Value.Should().BeOfType<QueryablePaging<TenantResponse>>();
        }
    }

    [Fact]
    public async Task GetTenantList_InvalidQuery_ReturnBadRequest()
    {
        // Arrange
        var query = new GridifyQuery { Page = 1, PageSize = 25, OrderBy = "nonexistentfield desc", };
        _tenantServiceMock.Setup(p => p.GetTenantsAsync(query, default)).ReturnsAsync(
            new QueryablePaging<TenantDto>(0,
                Enumerable.Empty<TenantDto>().AsQueryable()));

        // Act
        var actualResponse = await _controller.GetTenantList(query, default);

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
    public async Task GetDepartmentList_ValidQuery_ReturnSuccessStatusCode()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var query = new GridifyQuery { Page = 1, PageSize = 25, OrderBy = "name desc", };
        _tenantServiceMock.Setup(p => p.GetDepartmentsAsync(tenantId, query, default)).ReturnsAsync(
            new QueryablePaging<DepartmentDto>(0,
                Enumerable.Empty<DepartmentDto>().AsQueryable()));

        // Act
        var actualResponse = await _controller.GetDepartmentList(tenantId, query, default);

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
        var tenantId = Guid.NewGuid();
        var query = new GridifyQuery { Page = 1, PageSize = 25, OrderBy = "nonexistentfield desc", };
        _tenantServiceMock.Setup(p => p.GetDepartmentsAsync(tenantId, query, default)).ReturnsAsync(
            new QueryablePaging<DepartmentDto>(0,
                Enumerable.Empty<DepartmentDto>().AsQueryable()));

        // Act
        var actualResponse = await _controller.GetDepartmentList(tenantId, query, default);

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
    public async Task ExportTenantsAsync_ValidQuery_ReturnsFileContent(FileType fileType)
    {
        // Arrange
        var request = new ExportTenantsRequest(fileType, "America/New_York");
        _tenantServiceMock
            .Setup(x => x.ExportTenantsAsync(
                It.IsAny<FileType>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<byte>());

        // Act
        var actualResponse = await _controller.ExportTenantsAsync(request, default);

        // Assert
        using (new AssertionScope())
        {
            actualResponse.Should().NotBeNull();
            var actualResult = actualResponse as FileContentResult;
            actualResult.Should().NotBeNull();
            actualResult!.FileDownloadName.Should().Be($"tenants.{fileType.ToString().ToLowerInvariant()}");
            actualResult!.ContentType.Should().Be(fileType switch
            {
                FileType.Csv => "text/csv",
                FileType.Xlsx => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                _ => throw new InvalidOperationException()
            });
        }
    }

    [Fact]
    public void GetTenantList_MarkedWithCorrectHasPermissionsAttribute()
    {
        // Arrange
        var methodInfo = ((Func<GridifyQuery, CancellationToken, Task<IActionResult>>)_controller.GetTenantList).Method;

        // Act
        var hasPermissionsAttribute = methodInfo.GetCustomAttribute<HasPermissions>();

        // Assert
        using (new AssertionScope())
        {
            hasPermissionsAttribute.Should().NotBeNull();
            hasPermissionsAttribute?.Policy.Should().Be("Tenants.Read;Tenants.ReadWrite;Tenants.ReadExport");
        }
    }

    [Fact]
    public void ExportTenantsAsync_MarkedWithCorrectHasPermissionsAttribute()
    {
        // Arrange
        var methodInfo = ((Func<ExportTenantsRequest, CancellationToken, Task<IActionResult>>)_controller.ExportTenantsAsync).Method;

        // Act
        var hasPermissionsAttribute = methodInfo.GetCustomAttribute<HasPermissions>();

        // Assert
        using (new AssertionScope())
        {
            hasPermissionsAttribute.Should().NotBeNull();
            hasPermissionsAttribute?.Policy.Should().Be("Tenants.ReadExport");
        }
    }

    [Theory]
    [InlineData(FileType.Csv)]
    [InlineData(FileType.Xlsx)]
    public async Task ExportDepartmentsAsync_ValidQuery_ReturnsFileContent(FileType fileType)
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var request = new ExportDepartmentsRequest(fileType, "America/New_York");
        _tenantServiceMock
            .Setup(x => x.ExportDepartmentsAsync(
                tenantId,
                It.IsAny<FileType>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<byte>());

        // Act
        var actualResponse = await _controller.ExportDepartmentsAsync(tenantId, request, default);

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
}
