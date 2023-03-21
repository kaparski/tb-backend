using Bogus;
using FluentAssertions;
using FluentAssertions.Execution;
using Gridify;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TaxBeacon.API.Controllers.Users;
using TaxBeacon.API.Controllers.Users.Requests;
using TaxBeacon.API.Controllers.Users.Responses;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Services;
using TaxBeacon.UserManagement.Models;
using TaxBeacon.UserManagement.Services;

namespace TaxBeacon.API.UnitTests.Controllers.User;

public class UsersControllerTest
{
    private readonly Mock<IUserService> _userServiceMock;
    private readonly UsersController _controller;

    public UsersControllerTest()
    {
        _userServiceMock = new();
        _controller = new UsersController(_userServiceMock.Object);
    }

    [Fact]
    public async Task GetUserList_ValidQuery_ReturnSuccessStatusCode()
    {
        // Arrange
        var query = new GridifyQuery { Page = 1, PageSize = 25, OrderBy = "email desc", };
        _userServiceMock.Setup(p => p.GetUsersAsync(query, default)).ReturnsAsync(
            new QueryablePaging<UserDto>(0,
                Enumerable.Empty<UserDto>().AsQueryable()));

        // Act
        var actualResponse = await _controller.GetUserList(query, default);

        // Arrange
        using (new AssertionScope())
        {
            var actualResult = actualResponse as OkObjectResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResponse.Should().BeOfType<OkObjectResult>();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
            actualResult?.Value.Should().BeOfType<QueryablePaging<UserResponse>>();
        }
    }

    [Fact]
    public async Task GetUserList_InvalidQuery_ReturnBadRequest()
    {
        // Arrange
        var query = new GridifyQuery { Page = 1, PageSize = 25, OrderBy = "username desc", };
        _userServiceMock.Setup(p => p.GetUsersAsync(query, default)).ReturnsAsync(
            new QueryablePaging<UserDto>(0,
                Enumerable.Empty<UserDto>().AsQueryable()));

        // Act
        var actualResponse = await _controller.GetUserList(query, default);

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
    [InlineData(Status.Deactivated)]
    [InlineData(Status.Active)]
    public async Task UpdateUserStatusAsync_NewUserStatus_ReturnUpdatedUser(Status status)
    {
        // Arrange
        var userDto = TestData.TestUser.Generate();
        userDto.Status = Status.Deactivated;
        userDto.DeactivationDateTimeUtc = DateTime.UtcNow;

        _userServiceMock
            .Setup(service => service.UpdateUserStatusAsync(
                It.Is<Guid>(id => id == userDto.Id),
                It.IsAny<Status>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(userDto);

        // Act
        var actualResponse = await _controller.UpdateUserStatusAsync(userDto.Id, status, default);

        // Arrange
        using (new AssertionScope())
        {
            var actualResult = actualResponse.Result as OkObjectResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResponse.Should().BeOfType<ActionResult<UserResponse>>();
            actualResponse.Result.Should().BeOfType<OkObjectResult>();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
            actualResult?.Value.Should().BeOfType<UserResponse>();
            (actualResult?.Value as UserResponse)?.Id.Should().Be(userDto.Id);
        }
    }

    [Theory]
    [InlineData(FileType.Csv)]
    [InlineData(FileType.Xlsx)]
    public async Task ExportUsersAsync_ValidQuery_ReturnsFileContent(FileType fileType)
    {
        // Arrange
        var request = new ExportUsersRequest(fileType, "America/New_York");
        _userServiceMock
            .Setup(x => x.ExportUsersAsync(
                It.IsAny<Guid>(),
                It.IsAny<FileType>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<byte>());

        // Act
        var actualResponse = await _controller.ExportUsersAsync(request, default);

        // Assert
        using (new AssertionScope())
        {
            actualResponse.Should().NotBeNull();
            var actualResult = actualResponse as FileContentResult;
            actualResult.Should().NotBeNull();
            actualResult!.FileDownloadName.Should().Be($"users.{fileType.ToString().ToLowerInvariant()}");
            actualResult!.ContentType.Should().Be(fileType switch
            {
                FileType.Csv => "text/csv",
                FileType.Xlsx => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                _ => throw new InvalidOperationException()
            });
        }
    }

    private static class TestData
    {
        public static readonly Faker<UserDto> TestUser =
            new Faker<UserDto>()
                .RuleFor(u => u.Id, f => Guid.NewGuid())
                .RuleFor(u => u.FirstName, f => f.Name.FirstName())
                .RuleFor(u => u.LastName, f => f.Name.LastName())
                .RuleFor(u => u.Email, f => f.Internet.Email())
                .RuleFor(u => u.CreatedDateTimeUtc, f => DateTime.UtcNow)
                .RuleFor(u => u.ReactivationDateTimeUtc, f => null)
                .RuleFor(u => u.DeactivationDateTimeUtc, f => null)
                .RuleFor(u => u.Status, f => f.PickRandom<Status>());
    }
}
