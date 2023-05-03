﻿using FluentAssertions;
using FluentAssertions.Execution;
using Gridify;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OneOf.Types;
using System.Reflection;
using System.Security.Claims;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Controllers.ServiceAreas;
using TaxBeacon.API.Controllers.ServiceAreas.Requests;
using TaxBeacon.API.Controllers.ServiceAreas.Responses;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Services;
using TaxBeacon.UserManagement.Models;
using TaxBeacon.UserManagement.Services;

namespace TaxBeacon.API.UnitTests.Controllers.ServiceArea;

public class ServiceAreasControllerTest
{
    private readonly Mock<IServiceAreaService> _serviceAreaServiceMock;
    private readonly ServiceAreasController _controller;
    private readonly Guid _tenantId = Guid.NewGuid();

    public ServiceAreasControllerTest()
    {
        _serviceAreaServiceMock = new();

        _controller = new ServiceAreasController(_serviceAreaServiceMock.Object)
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

    [Fact]
    public async Task GetServiceAreaList_ValidQuery_ReturnSuccessStatusCode()
    {
        // Arrange
        var query = new GridifyQuery { Page = 1, PageSize = 25, OrderBy = "name desc", };
        _serviceAreaServiceMock.Setup(p => p.GetServiceAreasAsync(query, default)).ReturnsAsync(
            new QueryablePaging<ServiceAreaDto>(0,
                Enumerable.Empty<ServiceAreaDto>().AsQueryable()));

        // Act
        var actualResponse = await _controller.GetServiceAreaList(query, default);

        // Arrange
        using (new AssertionScope())
        {
            var actualResult = actualResponse as OkObjectResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResponse.Should().BeOfType<OkObjectResult>();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
            actualResult?.Value.Should().BeOfType<QueryablePaging<ServiceAreaResponse>>();
        }
    }

    [Fact]
    public async Task GetServiceAreaList_OrderByNonExistingProperty_ReturnBadRequestStatusCode()
    {
        // Arrange
        var query = new GridifyQuery { Page = 1, PageSize = 25, OrderBy = "email desc", };
        _serviceAreaServiceMock.Setup(p => p.GetServiceAreasAsync(query, default)).ReturnsAsync(
            new QueryablePaging<ServiceAreaDto>(0,
                Enumerable.Empty<ServiceAreaDto>().AsQueryable()));

        // Act
        var actualResponse = await _controller.GetServiceAreaList(query, default);

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
    public async Task GetServiceAreaList_TenantIdDoesNotExist_ReturnNotFoundStatusCode()
    {
        // Arrange
        var query = new GridifyQuery { Page = 1, PageSize = 25, OrderBy = "name desc", };
        _serviceAreaServiceMock.Setup(p => p.GetServiceAreasAsync(query, default))
            .ReturnsAsync(new NotFound());

        // Act
        var actualResponse = await _controller.GetServiceAreaList(query, default);

        // Arrange
        using (new AssertionScope())
        {
            var actualResult = actualResponse as NotFoundResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResponse.Should().BeOfType<NotFoundResult>();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }
    }

    [Theory]
    [InlineData(FileType.Csv)]
    [InlineData(FileType.Xlsx)]
    public async Task ExportServiceAreasAsync_ValidQuery_ReturnsFileContent(FileType fileType)
    {
        // Arrange
        var request = new ExportServiceAreasRequest(fileType, "America/New_York");
        _serviceAreaServiceMock
            .Setup(x => x.ExportServiceAreasAsync(
                It.IsAny<FileType>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<byte>());

        // Act
        var actualResponse = await _controller.ExportServiceAreasAsync(request, default);

        // Assert
        using (new AssertionScope())
        {
            actualResponse.Should().NotBeNull();
            var actualResult = actualResponse as FileContentResult;
            actualResult.Should().NotBeNull();
            actualResult!.FileDownloadName.Should().Be($"serviceareas.{fileType.ToString().ToLowerInvariant()}");
            actualResult!.ContentType.Should().Be(fileType switch
            {
                FileType.Csv => "text/csv",
                FileType.Xlsx => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                _ => throw new InvalidOperationException()
            });
        }
    }

    [Fact]
    public void GetServiceAreaList_MarkedWithCorrectHasPermissionsAttribute()
    {
        // Arrange
        var methodInfo = ((Func<GridifyQuery, CancellationToken, Task<IActionResult>>)_controller.GetServiceAreaList).Method;
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
    public void ExportServiceAreasAsync_MarkedWithCorrectHasPermissionsAttribute()
    {
        // Arrange
        var methodInfo = ((Func<ExportServiceAreasRequest, CancellationToken, Task<IActionResult>>)_controller.ExportServiceAreasAsync).Method;
        var permissions = new object[]
        {
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
    public async Task GetServiceAreaAsync_ServiceAreaExists_ShouldReturnSuccessfulStatusCode()
    {
        // Arrange
        _serviceAreaServiceMock.Setup(x => x.GetServiceAreaDetailsByIdAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync(new ServiceAreaDto());

        // Act
        var actualResponse = await _controller.GetServiceAreaAsync(Guid.NewGuid(), default);

        // Assert
        using (new AssertionScope())
        {
            var actualResult = actualResponse as OkObjectResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
            actualResult?.Value.Should().BeOfType<ServiceAreaResponse>();
        }
    }

    [Fact]
    public async Task GetServiceAreaAsync_ServiceAreaDoesNotExist_ShouldReturnNotFoundStatusCode()
    {
        // Arrange
        _serviceAreaServiceMock.Setup(x => x.GetServiceAreaDetailsByIdAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync(new NotFound());

        // Act
        var actualResponse = await _controller.GetServiceAreaAsync(Guid.NewGuid(), default);

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
    public void GetServiceAreaAsync_MarkedWithCorrectHasPermissionsAttribute()
    {
        // Arrange
        var methodInfo = ((Func<Guid, CancellationToken, Task<IActionResult>>)_controller.GetServiceAreaAsync).Method;
        var permissions = new object[] { Common.Permissions.ServiceAreas.Read, Common.Permissions.ServiceAreas.ReadWrite };

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
