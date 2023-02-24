using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using TaxBeacon.API.Controllers.Authorization;
using TaxBeacon.API.Controllers.Authorization.Requests;
using TaxBeacon.UserManagement.Services;

namespace TaxBeacon.API.UnitTests.Controllers.Authorization;

public class AuthorizationControllerTests
{
    private readonly Mock<ILogger<AuthorizationController>> _loggerMock;
    private readonly Mock<IUserService> _userServiceMock;
    private readonly AuthorizationController _controller;

    public AuthorizationControllerTests()
    {
        _loggerMock = new();
        _userServiceMock = new();

        _controller = new AuthorizationController(_loggerMock.Object, _userServiceMock.Object);
    }

    [Theory]
    [InlineData("1@1.com")]
    [InlineData("peter@parker.com")]
    [InlineData("bruce@gmail.com")]
    public async Task LoginAsync_ValidLoginRequest_ReturnSuccessStatusCode(string email)
    {
        //Arrange
        var loginRequest = new LoginRequest(email);

        //Act
        var actualResponse = await _controller.LoginAsync(loginRequest, default);

        //Assert
        actualResponse.Should().BeOfType<OkResult>();
        actualResponse.Should().NotBeNull();
        ((OkResult)actualResponse).StatusCode.Should().Be(StatusCodes.Status200OK);
    }
}
