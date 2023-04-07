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
    private readonly Mock<IPermissionsService> _permissionsServiceMock;

    public AuthorizationControllerTests()
    {
        _userServiceMock = new();
        _permissionsServiceMock = new();

        _controller = new AuthorizationController(_userServiceMock.Object, _permissionsServiceMock.Object);
    }

    [Fact]
    public async Task LoginAsync_ValidLoginRequest_ReturnSuccessStatusCode()
    {
        //Arrange
        var userDto = TestData.UserFaker.Generate();
        var loginRequest = new LoginRequest(userDto.Email);
        var permissions = new Faker().Random.WordsArray(4).AsReadOnly();

        _userServiceMock
            .Setup(service => service.LoginAsync(
                It.Is<MailAddress>(mailAddress => mailAddress.Address == loginRequest.Email),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(userDto);

        _permissionsServiceMock
            .Setup(service => service.GetPermissionsAsync(
                It.Is<Guid>(id => id == userDto.Id),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(permissions);

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
            responseValue?.UerId.Should().Be(userDto.Id);
            responseValue?.FullName.Should().Be(userDto.FullName);
            responseValue?.Permissions.Should().BeEquivalentTo(permissions);
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
        public static readonly Faker<UserDto> UserFaker =
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
