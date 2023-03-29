using Bogus;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net.Mail;
using TaxBeacon.API.Controllers.Authorization;
using TaxBeacon.API.Controllers.Authorization.Requests;
using TaxBeacon.API.Controllers.Authorization.Responses;
using TaxBeacon.Common.Enums;
using TaxBeacon.UserManagement.Models;
using TaxBeacon.UserManagement.Services;

namespace TaxBeacon.API.UnitTests.Controllers.Authorization;

public class AuthorizationControllerTests
{
    private readonly Mock<ILogger<AuthorizationController>> _loggerMock;
    private readonly Mock<IUserService> _userServiceMock;
    private readonly AuthorizationController _controller;
    private readonly Mock<IPermissionsService> _permissionsServiceMock;

    public AuthorizationControllerTests()
    {
        _loggerMock = new();
        _userServiceMock = new();
        _permissionsServiceMock = new();

        _controller = new AuthorizationController(_loggerMock.Object, _userServiceMock.Object, _permissionsServiceMock.Object);
    }

    [Theory]
    [InlineData("1@1.com")]
    [InlineData("peter@parker.com")]
    [InlineData("bruce@gmail.com")]
    public async Task LoginAsync_ValidLoginRequest_ReturnSuccessStatusCode(string email)
    {
        //Arrange
        var loginRequest = new LoginRequest(email);
        var userDto = TestData.TestUser.Generate();
        userDto.Email = email;
        _userServiceMock
            .Setup(service => service.LoginAsync(
                    It.Is<MailAddress>(mailAddress => mailAddress.Address == loginRequest.Email),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(userDto);

        //Act
        var actualResponse = await _controller.LoginAsync(loginRequest, default);

        //Assert
        using (new AssertionScope())
        {
            var actualResult = actualResponse.Result as OkObjectResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResponse.Should().BeOfType<ActionResult<LoginResponse>>();
            actualResponse.Result.Should().BeOfType<OkObjectResult>();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
            actualResult?.Value.Should().BeOfType<LoginResponse>();
            (actualResult?.Value as LoginResponse)?.FullName.Should().Be(userDto.FullName);
            (actualResult?.Value as LoginResponse)?.Permissions.Should().NotBeNull();
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
