﻿using Bogus;
using FluentAssertions;
using FluentAssertions.Execution;
using Gridify;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OneOf.Types;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Security.Claims;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Controllers.Departments.Responses;
using TaxBeacon.API.Controllers.Tenants;
using TaxBeacon.API.Controllers.Tenants.Requests;
using TaxBeacon.API.Controllers.Tenants.Responses;
using TaxBeacon.Common.Enums;
using TaxBeacon.UserManagement.Divisions;
using TaxBeacon.UserManagement.Divisions.Models;

namespace TaxBeacon.API.UnitTests.Controllers.Divisions;

public class DivisionControllerTest
{
    private readonly Mock<IDivisionsService> _divisionsServiceMock;
    private readonly DivisionsController _controller;

    public DivisionControllerTest()
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

    [Fact]
    public async Task GetDivisionsList_ValidQuery_ReturnSuccessStatusCode()
    {
        // Arrange
        var query = new GridifyQuery { Page = 1, PageSize = 25, OrderBy = "name desc", };
        _divisionsServiceMock.Setup(p => p.GetDivisionsAsync(query, default)).ReturnsAsync(
            new QueryablePaging<DivisionDto>(0,
                Enumerable.Empty<DivisionDto>().AsQueryable()));

        // Act
        var actualResponse = await _controller.GetDivisionsList(query, default);

        // Arrange
        using (new AssertionScope())
        {
            var actualResult = actualResponse as OkObjectResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResponse.Should().BeOfType<OkObjectResult>();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
            actualResult?.Value.Should().BeOfType<QueryablePaging<DivisionResponse>>();
        }
    }

    [Fact]
    public async Task GetDivisionsList_InvalidQuery_ReturnBadRequest()
    {
        // Arrange
        var query = new GridifyQuery { Page = 1, PageSize = 25, OrderBy = "nonexistentfield desc", };
        _divisionsServiceMock.Setup(p => p.GetDivisionsAsync(query, default)).ReturnsAsync(
            new QueryablePaging<DivisionDto>(0,
                Enumerable.Empty<DivisionDto>().AsQueryable()));

        // Act
        var actualResponse = await _controller.GetDivisionsList(query, default);

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
    public void GetDivisionsList_MarkedWithCorrectHasPermissionsAttribute()
    {
        // Arrange
        var methodInfo = ((Func<GridifyQuery, CancellationToken, Task<IActionResult>>)_controller.GetDivisionsList).Method;

        // Act
        var hasPermissionsAttribute = methodInfo.GetCustomAttribute<HasPermissions>();

        // Assert
        using (new AssertionScope())
        {
            hasPermissionsAttribute.Should().NotBeNull();
            hasPermissionsAttribute?.Policy.Should().Be("Divisions.Read;Divisions.ReadWrite;Divisions.ReadExport");
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
    public async Task GetDivisionUsers_ValidQuery_ShouldReturnSuccessStatusCode()
    {
        // Arrange
        var query = new GridifyQuery { Page = 1, PageSize = 25, OrderBy = "email desc", };
        _divisionsServiceMock.Setup(p => p.GetDivisionUsersAsync(It.IsAny<Guid>(), query, default))
            .ReturnsAsync(
                new QueryablePaging<DivisionUserDto>(0,
                    Enumerable.Empty<DivisionUserDto>().AsQueryable()));

        // Act
        var actualResponse = await _controller.GetDivisionUsers(query, new Guid(), default);

        // Arrange
        using (new AssertionScope())
        {
            var actualResult = actualResponse as OkObjectResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResponse.Should().BeOfType<OkObjectResult>();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
            actualResult?.Value.Should().BeOfType<QueryablePaging<DivisionUserResponse>>();
        }
    }

    [Fact]
    public async Task GetDivisionUsers_InvalidQuery_ShouldReturnBadRequest()
    {
        // Arrange
        var query = new GridifyQuery { Page = 1, PageSize = 25, OrderBy = "nonexistentfield desc", };
        _divisionsServiceMock.Setup(p => p.GetDivisionUsersAsync(It.IsAny<Guid>(), query, default))
            .ReturnsAsync(
                new QueryablePaging<DivisionUserDto>(0,
                    Enumerable.Empty<DivisionUserDto>().AsQueryable()));

        // Act
        var actualResponse = await _controller.GetDivisionUsers(query, new Guid(), default);

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
                .RuleFor(u => u.NumberOfUsers, f => f.Random.Number(1, 10));

        public static readonly Faker<DivisionDetailsDto> DivisionDetailsFaker =
            new Faker<DivisionDetailsDto>()
                .RuleFor(t => t.Id, f => Guid.NewGuid())
                .RuleFor(t => t.Name, f => f.Company.CompanyName())
                .RuleFor(t => t.CreatedDateTimeUtc, f => DateTime.UtcNow);

        public static readonly Faker<UpdateDivisionRequest> UpdateDivisionFaker =
            new Faker<UpdateDivisionRequest>()
                .CustomInstantiator(f => new UpdateDivisionRequest(f.Company.CompanyName(), f.Name.JobDescriptor(), new List<Guid>()));
    }
}