using Bogus;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Principal;
using TaxBeacon.API.Filters;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Exceptions;
using TaxBeacon.DAL.Entities;
using TaxBeacon.UserManagement.Models;
using TaxBeacon.UserManagement.Services;

namespace TaxBeacon.API.UnitTests.Filters;

public class AuthorizeFilterTests
{
    private readonly Mock<ILogger<AuthorizeFilter>> _loggerMock;
    private readonly Mock<IUserService> _userServiceMock;
    private readonly AuthorizeFilter _authorizeFilter;

    public AuthorizeFilterTests()
    {
        _loggerMock = new();
        _userServiceMock = new();
        _authorizeFilter = new AuthorizeFilter(_loggerMock.Object, _userServiceMock.Object);
    }

    [Fact]
    public async Task OnAuthorizationAsync_UserExistInDb_UserWithNewClaim()
    {
        // Arrange
        var user = TestData.TestUser.Generate();
        user.Status = Status.Active;
        var identity = new GenericIdentity("test", "test");
        identity.AddClaim(new Claim("preferred_username", user.Email));
        var contextUser = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext { User = contextUser };
        var actionContext = new ActionContext(httpContext, Mock.Of<RouteData>(), Mock.Of<ActionDescriptor>());
        var authorizationFilterContext = new AuthorizationFilterContext(
            actionContext, new List<IFilterMetadata>());

        _userServiceMock
            .Setup(service => service.GetUserByEmailAsync(
                It.Is<MailAddress>(mailAddress =>
                    mailAddress.Address.Equals(user.Email, StringComparison.OrdinalIgnoreCase)),
                default))
            .ReturnsAsync(user);

        // Act
        await _authorizeFilter.OnAuthorizationAsync(authorizationFilterContext);

        // Assert
        httpContext.User.HasClaim(claim => claim.Type.Equals("userId", StringComparison.OrdinalIgnoreCase))
            .Should()
            .BeTrue();
    }

    [Fact]
    public async Task OnAuthorizationAsync_UserExistInDbAndHasDeactivatedStatus_ReturnForbidResult()
    {
        // Arrange
        var user = TestData.TestUser.Generate();
        user.Status = Status.Deactivated;
        var identity = new GenericIdentity("test", "test");
        identity.AddClaim(new Claim("preferred_username", user.Email));
        var contextUser = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext { User = contextUser };
        var actionContext = new ActionContext(httpContext, Mock.Of<RouteData>(), Mock.Of<ActionDescriptor>());
        var authorizationFilterContext = new AuthorizationFilterContext(
            actionContext, new List<IFilterMetadata>());

        _userServiceMock
            .Setup(service => service.GetUserByEmailAsync(
                It.Is<MailAddress>(mailAddress =>
                    mailAddress.Address.Equals(user.Email, StringComparison.OrdinalIgnoreCase)),
                default))
            .ReturnsAsync(user);

        // Act
        await _authorizeFilter.OnAuthorizationAsync(authorizationFilterContext);

        // Assert
        authorizationFilterContext.Result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task OnAuthorizationAsync_UserExistInDbAndHasDeactivationDateTimeUtc_ReturnForbidResult()
    {
        // Arrange
        var user = TestData.TestUser.Generate();
        user.Status = Status.Active;
        user.DeactivationDateTimeUtc = DateTime.UtcNow;
        var identity = new GenericIdentity("test", "test");
        identity.AddClaim(new Claim("preferred_username", user.Email));
        var contextUser = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext { User = contextUser };
        var actionContext = new ActionContext(httpContext, Mock.Of<RouteData>(), Mock.Of<ActionDescriptor>());
        var authorizationFilterContext = new AuthorizationFilterContext(
            actionContext, new List<IFilterMetadata>());

        _userServiceMock
            .Setup(service => service.GetUserByEmailAsync(
                It.Is<MailAddress>(mailAddress =>
                    mailAddress.Address.Equals(user.Email, StringComparison.OrdinalIgnoreCase)),
                default))
            .ReturnsAsync(user);

        // Act
        await _authorizeFilter.OnAuthorizationAsync(authorizationFilterContext);

        // Assert
        authorizationFilterContext.Result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task OnAuthorizationAsync_UserNotInDb_ReturnForbidResult()
    {
        // Arrange
        var faker = new Faker();
        var email = faker.Internet.Email(faker.Name.FirstName(), faker.Name.LastName());
        var identity = new GenericIdentity("test", "test");
        identity.AddClaim(new Claim("preferred_username", email));
        var contextUser = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext { User = contextUser };
        var actionContext = new ActionContext(httpContext, Mock.Of<RouteData>(), Mock.Of<ActionDescriptor>());
        var authorizationFilterContext = new AuthorizationFilterContext(
            actionContext, new List<IFilterMetadata>());

        _userServiceMock
            .Setup(service => service.GetUserByEmailAsync(
                It.Is<MailAddress>(mailAddress =>
                    mailAddress.Address.Equals(email, StringComparison.OrdinalIgnoreCase)),
                default))
            .ThrowsAsync(new NotFoundException(nameof(User), email));

        // Act
        await _authorizeFilter.OnAuthorizationAsync(authorizationFilterContext);

        // Assert
        authorizationFilterContext.Result.Should().BeOfType<UnauthorizedResult>();
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
