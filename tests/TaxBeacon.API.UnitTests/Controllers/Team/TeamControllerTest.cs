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
using TaxBeacon.API.Controllers.Teams;
using TaxBeacon.API.Controllers.Teams.Requests;
using TaxBeacon.API.Controllers.Teams.Responses;
using TaxBeacon.API.Controllers.Tenants.Responses;
using TaxBeacon.Common.Enums;
using TaxBeacon.UserManagement.Models;
using TaxBeacon.UserManagement.Services;

namespace TaxBeacon.API.UnitTests.Controllers.Team;

public class TeamControllerTest
{
    private readonly Mock<ITeamService> _teamServiceMock;
    private readonly TeamsController _controller;

    public TeamControllerTest()
    {
        _teamServiceMock = new();
        _controller = new TeamsController(_teamServiceMock.Object)
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
    public async Task GetTeamsList_ValidQuery_ReturnSuccessStatusCode()
    {
        // Arrange
        var query = new GridifyQuery { Page = 1, PageSize = 25, OrderBy = "name desc", };
        _teamServiceMock.Setup(p => p.GetTeamsAsync(query, default)).ReturnsAsync(
            new QueryablePaging<TeamDto>(0,
                Enumerable.Empty<TeamDto>().AsQueryable()));

        // Act
        var actualResponse = await _controller.GetTeamList(query, default);

        // Arrange
        using (new AssertionScope())
        {
            var actualResult = actualResponse as OkObjectResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResponse.Should().BeOfType<OkObjectResult>();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
            actualResult?.Value.Should().BeOfType<QueryablePaging<TeamResponse>>();
        }
    }

    [Fact]
    public async Task GetTeamList_InvalidQuery_ReturnBadRequest()
    {
        // Arrange
        var query = new GridifyQuery { Page = 1, PageSize = 25, OrderBy = "nonexistentfield desc", };
        _teamServiceMock.Setup(p => p.GetTeamsAsync(query, default)).ReturnsAsync(
            new QueryablePaging<TeamDto>(0,
                Enumerable.Empty<TeamDto>().AsQueryable()));

        // Act
        var actualResponse = await _controller.GetTeamList(query, default);

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
    public async Task ExportTeamsAsync_ValidQuery_ReturnsFileContent(FileType fileType)
    {
        // Arrange
        var request = new ExportTeamsRequest(fileType, "America/New_York");
        _teamServiceMock
            .Setup(x => x.ExportTeamsAsync(
                It.IsAny<FileType>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<byte>());

        // Act
        var actualResponse = await _controller.ExportTeamsAsync(request, default);

        // Assert
        using (new AssertionScope())
        {
            actualResponse.Should().NotBeNull();
            var actualResult = actualResponse as FileContentResult;
            actualResult.Should().NotBeNull();
            actualResult!.FileDownloadName.Should().Be($"teams.{fileType.ToString().ToLowerInvariant()}");
            actualResult!.ContentType.Should().Be(fileType switch
            {
                FileType.Csv => "text/csv",
                FileType.Xlsx => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                _ => throw new InvalidOperationException()
            });
        }
    }

    [Fact]
    public void GetTeamList_MarkedWithCorrectHasPermissionsAttribute()
    {
        // Arrange
        var methodInfo = ((Func<GridifyQuery, CancellationToken, Task<IActionResult>>)_controller.GetTeamList).Method;

        // Act
        var hasPermissionsAttribute = methodInfo.GetCustomAttribute<HasPermissions>();

        // Assert
        using (new AssertionScope())
        {
            hasPermissionsAttribute.Should().NotBeNull();
            hasPermissionsAttribute?.Policy.Should().Be("Teams.Read;Teams.ReadExport;Teams.ReadWrite");
        }
    }

    [Fact]
    public void ExportTeamAsync_MarkedWithCorrectHasPermissionsAttribute()
    {
        // Arrange
        var methodInfo = ((Func<ExportTeamsRequest, CancellationToken, Task<IActionResult>>)_controller.ExportTeamsAsync).Method;

        // Act
        var hasPermissionsAttribute = methodInfo.GetCustomAttribute<HasPermissions>();

        // Assert
        using (new AssertionScope())
        {
            hasPermissionsAttribute.Should().NotBeNull();
            hasPermissionsAttribute?.Policy.Should().Be("Teams.ReadExport");
        }
    }

    [Fact]
    public async Task GetTeamDetailsAsync_TeamExists_ShouldReturnSuccessfulStatusCode()
    {
        // Arrange
        _teamServiceMock.Setup(x => x.GetTeamDetailsAsync(It.IsAny<Guid>(), default)).ReturnsAsync(new TeamDetailsDto());

        // Act
        var actualResponse = await _controller.GetTeamDetails(Guid.NewGuid(), default);

        // Assert
        using (new AssertionScope())
        {
            var actualResult = actualResponse as OkObjectResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
            actualResult?.Value.Should().BeOfType<TeamDetailsResponse>();
        }
    }

    [Fact]
    public async Task GetTeamDetailsAsync_TeamDoesNotExist_ShouldReturnNotFoundStatusCode()
    {
        // Arrange
        _teamServiceMock.Setup(x => x.GetTeamDetailsAsync(It.IsAny<Guid>(), default)).ReturnsAsync(new NotFound());

        // Act
        var actualResponse = await _controller.GetTeamDetails(Guid.NewGuid(), default);

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
    public async Task UpdateTeamAsync_InvalidTeamId_ReturnsNotFoundResponse()
    {
        // Arrange
        var request = TestData.UpdateTeamFaker.Generate();
        _teamServiceMock
            .Setup(service => service.UpdateTeamAsync(
                It.IsAny<Guid>(),
                It.IsAny<UpdateTeamDto>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NotFound());

        // Act
        var actualResponse = await _controller.UpdateTeamAsync(Guid.NewGuid(), request, default);

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
    public async Task UpdateTeamAsync_ValidTeamId_ReturnsUpdatedTeam()
    {
        // Arrange
        var teamDto = TestData.TeamFaker.Generate();
        var request = TestData.UpdateTeamFaker.Generate();
        _teamServiceMock
            .Setup(service => service.UpdateTeamAsync(
                It.Is<Guid>(id => id == teamDto.Id),
                It.IsAny<UpdateTeamDto>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(teamDto);

        // Act
        var actualResponse = await _controller.UpdateTeamAsync(teamDto.Id, request, default);

        // Arrange
        using (new AssertionScope())
        {
            var actualResult = actualResponse as OkObjectResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResponse.Should().BeOfType<OkObjectResult>();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
            actualResult?.Value.Should().BeOfType<TeamResponse>();
        }
    }

    [Fact]
    public void UpdateTeamAsync_MarkedWithCorrectHasPermissionsAttribute()
    {
        // Arrange
        var methodInfo = ((Func<Guid, UpdateTeamRequest, CancellationToken, Task<IActionResult>>)_controller.UpdateTeamAsync).Method;
        var permissions = new object[] { Common.Permissions.Teams.ReadWrite };

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
    public void GetTeamUsers_MarkedWithCorrectHasPermissionsAttribute()
    {
        // Arrange
        var methodInfo = ((Func<GridifyQuery, Guid, CancellationToken, Task<IActionResult>>)_controller.GetTeamUsers).Method;

        // Act
        var hasPermissionsAttribute = methodInfo.GetCustomAttribute<HasPermissions>();

        // Assert
        using (new AssertionScope())
        {
            hasPermissionsAttribute.Should().NotBeNull();
            hasPermissionsAttribute?.Policy.Should().Be("Teams.Read;Teams.ReadWrite");
        }
    }

    [Fact]
    public async Task GetTeamUsers_ValidQuery_ShouldReturnSuccessStatusCode()
    {
        // Arrange
        var query = new GridifyQuery { Page = 1, PageSize = 25, OrderBy = "email desc", };
        _teamServiceMock.Setup(p => p.GetTeamUsersAsync(It.IsAny<Guid>(), query, default))
            .ReturnsAsync(
            new QueryablePaging<TeamUserDto>(0,
                Enumerable.Empty<TeamUserDto>().AsQueryable()));

        // Act
        var actualResponse = await _controller.GetTeamUsers(query, new Guid(), default);

        // Arrange
        using (new AssertionScope())
        {
            var actualResult = actualResponse as OkObjectResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResponse.Should().BeOfType<OkObjectResult>();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
            actualResult?.Value.Should().BeOfType<QueryablePaging<TeamUserResponse>>();
        }
    }

    [Fact]
    public async Task GetTeamUsers_InvalidQuery_ShouldReturnBadRequest()
    {
        // Arrange
        var query = new GridifyQuery { Page = 1, PageSize = 25, OrderBy = "nonexistentfield desc", };
        _teamServiceMock.Setup(p => p.GetTeamUsersAsync(It.IsAny<Guid>(), query, default))
            .ReturnsAsync(
            new QueryablePaging<TeamUserDto>(0,
                Enumerable.Empty<TeamUserDto>().AsQueryable()));

        // Act
        var actualResponse = await _controller.GetTeamUsers(query, new Guid(), default);

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
    public async Task GetTeamUsers_TeamDoesNotExist_ShouldReturnNotFoundStatusCode()
    {
        // Arrange
        var query = new GridifyQuery { Page = 1, PageSize = 25, OrderBy = "email desc", };
        _teamServiceMock
            .Setup(x => x.GetTeamUsersAsync(It.IsAny<Guid>(), query, default))
            .ReturnsAsync(new NotFound());

        // Act
        var actualResponse = await _controller.GetTeamUsers(query, Guid.NewGuid(), default);

        // Assert
        using (new AssertionScope())
        {
            var actualResult = actualResponse as NotFoundResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }
    }

    private static class TestData
    {
        public static readonly Faker<TeamDto> TeamFaker =
            new Faker<TeamDto>()
                .RuleFor(u => u.Id, f => Guid.NewGuid())
                .RuleFor(u => u.Name, f => f.Company.CompanyName())
                .RuleFor(u => u.Description, f => f.Name.JobDescriptor())
                .RuleFor(u => u.CreatedDateTimeUtc, f => DateTime.UtcNow)
                .RuleFor(u => u.NumberOfUsers, f => f.Random.Number(1, 10));

        public static readonly Faker<UpdateTeamRequest> UpdateTeamFaker =
            new Faker<UpdateTeamRequest>()
                .CustomInstantiator(f => new UpdateTeamRequest(f.Company.CompanyName(), f.Name.JobDescriptor()));
    }
}
