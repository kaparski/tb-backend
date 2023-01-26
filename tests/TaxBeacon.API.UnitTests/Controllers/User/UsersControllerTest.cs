using FluentAssertions;
using Gridify;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TaxBeacon.API.Controllers.Users;
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
        var query = new GridifyQuery
        {
            Page = 1,
            PageSize = 25,
            OrderBy = "username desc",
        };
        _userServiceMock.Setup(p => p.GetUsersAsync(query, default)).ReturnsAsync(
            new QueryablePaging<UserDto>(0,
                Enumerable.Empty<UserDto>().AsQueryable()));

        // Act
        var actualResponse = await _controller.GetUserList(query, default);

        // Assert
        actualResponse.Should().BeOfType<OkObjectResult>();
        actualResponse.Should().NotBeNull();
    }
}
