using Bogus;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OneOf.Types;
using System.Security.Claims;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Controllers.Users;
using TaxBeacon.API.Controllers.Users.Requests;
using TaxBeacon.API.Controllers.Users.Responses;
using TaxBeacon.Common.Enums;
using TaxBeacon.UserManagement.Models;
using TaxBeacon.UserManagement.Services;

namespace TaxBeacon.API.UnitTests.Controllers.User;

public class UserControllerTests
{
    private readonly Mock<IUserService> _userServiceMock;
    private readonly UserController _controller;

    public UserControllerTests()
    {
        _userServiceMock = new();
        _controller = new UserController(_userServiceMock.Object)
        {
            ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new[]
                    {
                        new ClaimsIdentity(new[]
                        {
                            new Claim(Claims.TenantId, Guid.NewGuid().ToString())
                        })
                    })
                }
            }
        };
    }

    [Theory]
    [InlineData(Status.Deactivated)]
    [InlineData(Status.Active)]
    public async Task UpdateUserStatusAsync_NewUserStatus_ReturnsUpdatedUser(Status status)
    {
        // Arrange
        var userDto = TestData.UserFaker.Generate();
        userDto.Status = Status.Deactivated;
        userDto.DeactivationDateTimeUtc = DateTime.UtcNow;

        _userServiceMock
            .Setup(service => service.UpdateUserStatusAsync(
                It.IsAny<Guid>(),
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

    [Fact]
    public async Task UpdateUserAsync_InvalidUserId_ReturnsNotFoundResponse()
    {
        // Arrange
        var request = TestData.UpdateUserFaker.Generate();
        _userServiceMock
            .Setup(service => service.UpdateUserByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<UpdateUserDto>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NotFound());

        // Act
        var actualResponse = await _controller.UpdateUserAsync(Guid.NewGuid(), request, default);

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
    public async Task UpdateUserAsync_ValidUserId_ReturnsUpdatedUser()
    {
        // Arrange
        var userDto = TestData.UserFaker.Generate();
        var request = TestData.UpdateUserFaker.Generate();
        _userServiceMock
            .Setup(service => service.UpdateUserByIdAsync(
                It.IsAny<Guid>(),
                It.Is<Guid>(id => id == userDto.Id),
                It.IsAny<UpdateUserDto>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(userDto);

        // Act
        var actualResponse = await _controller.UpdateUserAsync(userDto.Id, request, default);

        // Arrange
        using (new AssertionScope())
        {
            var actualResult = actualResponse as OkObjectResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResponse.Should().BeOfType<OkObjectResult>();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
            actualResult?.Value.Should().BeOfType<UserResponse>();
        }
    }

    private static class TestData
    {
        public static readonly Faker<UserDto> UserFaker =
            new Faker<UserDto>()
                .RuleFor(u => u.Id, f => Guid.NewGuid())
                .RuleFor(u => u.FirstName, f => f.Name.FirstName())
                .RuleFor(u => u.LegalName, f => f.Name.FirstName())
                .RuleFor(u => u.LastName, f => f.Name.LastName())
                .RuleFor(u => u.Email, f => f.Internet.Email())
                .RuleFor(u => u.CreatedDateTimeUtc, f => DateTime.UtcNow)
                .RuleFor(u => u.ReactivationDateTimeUtc, f => null)
                .RuleFor(u => u.DeactivationDateTimeUtc, f => null)
                .RuleFor(u => u.Status, f => f.PickRandom<Status>());

        public static readonly Faker<UpdateUserRequest> UpdateUserFaker =
            new Faker<UpdateUserRequest>()
                .CustomInstantiator(f => new UpdateUserRequest(f.Name.FirstName(), f.Name.FirstName(), f.Name.LastName()));
    }
}
