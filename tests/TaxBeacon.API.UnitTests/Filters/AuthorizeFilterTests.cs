using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using System.Security.Principal;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Filters;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.API.UnitTests.Filters;

public class AuthorizeFilterTests
{
    private readonly Mock<ILogger<AuthorizeFilter>> _loggerMock;
    private readonly AuthorizeFilter _authorizeFilter;

    public AuthorizeFilterTests()
    {
        _loggerMock = new();
        _authorizeFilter = new AuthorizeFilter(_loggerMock.Object);
    }

    [Fact]
    public async Task OnAuthorizationAsync_UserIdExistsInClaims_SuccessfullyAuthorized()
    {
        // Arrange
        var contextUser = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(Claims.UserId, Guid.NewGuid().ToString())
        }));

        var httpContext = new DefaultHttpContext { User = contextUser };
        var actionContext = new ActionContext(httpContext, Mock.Of<RouteData>(), Mock.Of<ActionDescriptor>());
        var authorizationFilterContext = new AuthorizationFilterContext(
            actionContext, new List<IFilterMetadata>());

        // Act
        await _authorizeFilter.OnAuthorizationAsync(authorizationFilterContext);

        // Assert
        authorizationFilterContext.Result.Should().BeNull();
    }

    [Fact]
    public async Task OnAuthorizationAsync_UserIdIsNotExistsInClaims_ReturnsUnauthorizedResult()
    {
        // Arrange
        var contextUser = new ClaimsPrincipal(new ClaimsIdentity());
        var httpContext = new DefaultHttpContext { User = contextUser };
        var actionContext = new ActionContext(httpContext, Mock.Of<RouteData>(), Mock.Of<ActionDescriptor>());
        var authorizationFilterContext = new AuthorizationFilterContext(
            actionContext, new List<IFilterMetadata>());

        // Act
        await _authorizeFilter.OnAuthorizationAsync(authorizationFilterContext);

        // Assert
        authorizationFilterContext.Result.Should().BeOfType<UnauthorizedResult>();
    }
}
