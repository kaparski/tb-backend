using Bogus;
using FluentAssertions;
using FluentAssertions.Execution;
using Gridify;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Data;
using TaxBeacon.API.Controllers.Users;
using TaxBeacon.API.Controllers.Users.Responses;
using TaxBeacon.Common.Enums;
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
        var query = new GridifyQuery { Page = 1, PageSize = 25, OrderBy = "username desc", };
        _userServiceMock.Setup(p => p.GetUsersAsync(query, default)).ReturnsAsync(
            new QueryablePaging<UserDto>(0,
                Enumerable.Empty<UserDto>().AsQueryable()));

        // Act
        var actualResponse = await _controller.GetUserList(query, default);

        // Assert
        actualResponse.Should().BeOfType<ActionResult<QueryablePaging<UserResponse>>>();
        actualResponse.Should().NotBeNull();
    }

    [Theory]
    [InlineData(UserStatus.Deactivated)]
    [InlineData(UserStatus.Active)]
    public async Task UpdateUserStatusAsync_NewUserStatus_ReturnUpdatedUser(UserStatus userStatus)
    {
        // Arrange
        var userDto = TestData.TestUser.Generate();
        userDto.UserStatus = UserStatus.Deactivated;
        userDto.DeactivationDateTimeUtc = DateTime.UtcNow;

        _userServiceMock
            .Setup(service => service.UpdateUserStatusAsync(
                It.Is<Guid>(id => id == userDto.Id),
                It.IsAny<UserStatus>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(userDto);

        // Act
        var actualResponse = await _controller.UpdateUserStatusAsync(userDto.Id, userStatus, default);

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

    private static class TestData
    {
        public static readonly Faker<UserDto> TestUser =
            new Faker<UserDto>()
                .RuleFor(u => u.Id, f => Guid.NewGuid())
                .RuleFor(u => u.FirstName, f => f.Name.FirstName())
                .RuleFor(u => u.LastName, f => f.Name.LastName())
                .RuleFor(u => u.Email, f => f.Internet.Email())
                .RuleFor(u => u.CreatedDateUtc, f => DateTime.UtcNow)
                .RuleFor(u => u.ReactivationDateTimeUtc, f => null)
                .RuleFor(u => u.DeactivationDateTimeUtc, f => null)
                .RuleFor(u => u.UserStatus, f => f.PickRandom<UserStatus>());
    }
}
