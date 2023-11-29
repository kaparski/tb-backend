using Bogus;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OneOf.Types;
using System.Diagnostics.CodeAnalysis;
using System.Net.Mail;
using System.Security.Claims;
using TaxBeacon.Administration.Users;
using TaxBeacon.Administration.Users.Models;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Controllers.Authorization;
using TaxBeacon.API.Controllers.Authorization.Responses;

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
        var email = new MailAddress("test@test.com");

        _userServiceMock
            .Setup(service => service.LoginAsync(
                email,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(loginUserDto);

        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new[] { new ClaimsIdentity(new[] { new Claim(Claims.EmailClaimName, email.Address) }) })
            }
        };

        //Act
        var actualResponse = await _controller.LoginAsync(default);

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
        _userServiceMock
            .Setup(service => service.LoginAsync(
                It.IsAny<MailAddress>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NotFound());

        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new[] { new ClaimsIdentity(new[] { new Claim(Claims.EmailClaimName, "test@test.com") }) })
            }
        };

        // Act
        var actualResponse = await _controller.LoginAsync(default);

        // Arrange
        using (new AssertionScope())
        {
            var actualResult = actualResponse as NotFoundResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    private static class TestData
    {
        public static readonly Faker<LoginUserDto> LoginUserDtoFaker =
            new Faker<LoginUserDto>()
                .CustomInstantiator(f => new LoginUserDto(
                    Guid.NewGuid(),
                    f.Name.FullName(),
                    f.Random.WordsArray(4).AsReadOnly(),
                    f.Random.Bool(),
                    f.Random.Bool(),
                    Guid.NewGuid(),
                    f.Company.CompanyName()));
    }
}

