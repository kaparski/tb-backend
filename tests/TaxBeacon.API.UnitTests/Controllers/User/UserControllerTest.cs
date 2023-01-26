using FluentAssertions;
using Gridify;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Moq;
using System.Net;
using TaxBeacon.API.Controllers.Authorization;
using TaxBeacon.API.Controllers.Users;
using TaxBeacon.API.Controllers.Users.Responses;
using TaxBeacon.UserManagement.Models;
using TaxBeacon.UserManagement.Services;

namespace TaxBeacon.API.UnitTests.Controllers.User;

public class UserControllerTest
{
    private readonly Mock<IUserService> _userServiceMock;
    private readonly UserController _controller;

    public UserControllerTest()
    {
        _userServiceMock = new();

        _controller = new UserController(_userServiceMock.Object);
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
