using Bogus;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OneOf.Types;
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
    private readonly Mock<IUserService> _userServiceMock;
    private readonly AuthorizationController _controller;

    public AuthorizationControllerTests()
    {
        _userServiceMock = new();

        _controller = new AuthorizationController(_userServiceMock.Object);
    }

    [Fact]
    public async Task LoginAsync_ValidLoginRequest_ReturnSuccessStatusCode()
    {
        //Arrange
        var loginUserDto = TestData.LoginUserDtoFaker.Generate();
        var loginRequest = new LoginRequest(new Faker().Internet.Email());

        _userServiceMock
            .Setup(service => service.LoginAsync(
                It.Is<MailAddress>(mailAddress => mailAddress.Address == loginRequest.Email),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(loginUserDto);

        //Act
        var actualResponse = await _controller.LoginAsync(loginRequest, default);

        //Assert
        using (new AssertionScope())
        {
            var actualResult = actualResponse as OkObjectResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResponse.Should().BeOfType<OkObjectResult>();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status200OK);

            var responseValue = actualResult?.Value as LoginResponse;
            actualResult?.Value.Should().BeOfType<LoginResponse>();
            responseValue?.Should().BeEquivalentTo(loginUserDto);
        }
    }

    [Fact]
    public async Task UpdateUserAsync_InvalidUserId_ReturnsNotFoundResponse()
    {
        // Arrange
        var loginRequest = new LoginRequest(new Faker().Internet.Email());
        _userServiceMock
            .Setup(service => service.LoginAsync(
                It.Is<MailAddress>(mailAddress => mailAddress.Address == loginRequest.Email),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NotFound());

        // Act
        var actualResponse = await _controller.LoginAsync(loginRequest, default);

        // Arrange
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
        public static readonly Faker<LoginUserDto> LoginUserDtoFaker =
            new Faker<LoginUserDto>()
                .CustomInstantiator(f => new LoginUserDto(
                    Guid.NewGuid(),
                    f.Name.FullName(),
                    f.Random.WordsArray(4).AsReadOnly(),
                    f.Random.Bool()));
    }
}

