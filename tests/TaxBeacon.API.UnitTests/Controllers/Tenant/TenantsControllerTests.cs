﻿using Bogus;
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
using TaxBeacon.API.Controllers.Tenants;
using TaxBeacon.API.Controllers.Tenants.Requests;
using TaxBeacon.API.Controllers.Tenants.Responses;
using TaxBeacon.API.Controllers.Users.Requests;
using TaxBeacon.Common.Enums;
using TaxBeacon.UserManagement.Models;
using TaxBeacon.UserManagement.Services;

namespace TaxBeacon.API.UnitTests.Controllers.Tenant;

public class TenantsControllerTests
{
    private readonly Mock<ITenantService> _tenantServiceMock;
    private readonly TenantsController _controller;

    public TenantsControllerTests()
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

    [Fact]
    public async Task GetTenantAsync_TenantExists_ShouldReturnSuccessfulStatusCode()
    {
        // Arrange
        _tenantServiceMock.Setup(x => x.GetTenantByIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync(new TenantDto());

        // Act
        var actualResponse = await _controller.GetTenantAsync(Guid.NewGuid(), default);

        // Assert
        using (new AssertionScope())
        {
            var actualResult = actualResponse as OkObjectResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
            actualResult?.Value.Should().BeOfType<TenantResponse>();
        }
    }

    [Fact]
    public async Task GetTenantAsync_TenantDoesNotExist_ShouldReturnNotFoundStatusCode()
    {
        // Arrange
        _tenantServiceMock.Setup(x => x.GetTenantByIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync(new NotFound());

        // Act
        var actualResponse = await _controller.GetTenantAsync(Guid.NewGuid(), default);

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
    public void GetTenantAsync_MarkedWithCorrectHasPermissionsAttribute()
    {
        // Arrange
        var methodInfo = ((Func<Guid, CancellationToken, Task<IActionResult>>)_controller.GetTenantAsync).Method;
        var permissions = new object[] { Common.Permissions.Tenants.Read, Common.Permissions.Tenants.ReadWrite };

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
        _tenantServiceMock.Setup(x =>
                x.GetActivityHistoryAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>(), default))
            .ReturnsAsync(new ActivityDto(0, new ActivityItemDto[] { }));

        // Act
        var actualResponse = await _controller.GetActivityHistoryAsync(Guid.NewGuid(), new TenantActivityHistoryRequest(1, 1), default);

        // Assert
        using (new AssertionScope())
        {
            var actualResult = actualResponse as OkObjectResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
            actualResult?.Value.Should().BeOfType<TenantActivityHistoryResponse>();
        }
    }

    [Fact]
    public async Task GetActivityHistoryAsync_TenantDoesNotExist_ShouldReturnNotFoundStatusCode()
    {
        // Arrange
        _tenantServiceMock.Setup(x =>
                x.GetActivityHistoryAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>(), default))
            .ReturnsAsync(new NotFound());

        // Act
        var actualResponse =
            await _controller.GetActivityHistoryAsync(Guid.NewGuid(), new TenantActivityHistoryRequest(1, 1), default);

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
        var methodInfo = ((Func<Guid, TenantActivityHistoryRequest, CancellationToken, Task<IActionResult>>)_controller.GetActivityHistoryAsync).Method;
        var permissions = new object[] { Common.Permissions.Tenants.Read, Common.Permissions.Tenants.ReadWrite };

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
    public async Task UpdateTenantAsync_TenantExistsAndRequestIsValid_ShouldReturnSuccessfulStatusCode()
    {
        // Arrange
        var request = TestData.TestUpdateTenantRequest.Generate();
        var tenantDto = TestData.TestTenant.Generate();
        _tenantServiceMock.Setup(x => x.UpdateTenantAsync(It.Is<Guid>(id => id == tenantDto.Id), It.IsAny<UpdateTenantDto>(), default))
            .ReturnsAsync(tenantDto);

        // Act
        var actualResponse = await _controller.UpdateTenantAsync(tenantDto.Id, request, default);

        // Assert
        using (new AssertionScope())
        {
            var actualResult = actualResponse as OkObjectResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
            actualResult?.Value.Should().BeOfType<TenantResponse>();
        }
    }

    [Fact]
    public async Task UpdateTenantAsync_TenantDoesNotExists_ShouldReturnSuccessfulStatusCode()
    {
        // Arrange
        var request = TestData.TestUpdateTenantRequest.Generate();
        _tenantServiceMock.Setup(x => x.UpdateTenantAsync(It.IsAny<Guid>(), It.IsAny<UpdateTenantDto>(), default))
            .ReturnsAsync(new NotFound());

        // Act
        var actualResponse = await _controller.UpdateTenantAsync(Guid.NewGuid(), request, default);

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
    public void UpdateTenantAsync_MarkedWithCorrectHasPermissionsAttribute()
    {
        // Arrange
        var methodInfo = ((Func<Guid, UpdateTenantRequest, CancellationToken, Task<IActionResult>>)_controller.UpdateTenantAsync).Method;
        var permissions = new object[] { Common.Permissions.Tenants.ReadWrite };

        // Act
        var hasPermissionsAttribute = methodInfo.GetCustomAttribute<HasPermissions>();

        // Assert
        using (new AssertionScope())
        {
            hasPermissionsAttribute.Should().NotBeNull();
            hasPermissionsAttribute?.Policy.Should().Be(string.Join(";", permissions.Select(x => $"{x.GetType().Name}.{x}")));
        }
    }

    private static class TestData
    {
        public static readonly Faker<TenantDto> TestTenant =
            new Faker<TenantDto>()
                .RuleFor(t => t.Id, f => Guid.NewGuid())
                .RuleFor(t => t.Name, f => f.Company.CompanyName())
                .RuleFor(t => t.CreatedDateTimeUtc, f => DateTime.UtcNow);

        public static readonly Faker<UpdateTenantRequest> TestUpdateTenantRequest =
            new Faker<UpdateTenantRequest>().CustomInstantiator(f => new UpdateTenantRequest(f.Company.CompanyName()));
    }
}
