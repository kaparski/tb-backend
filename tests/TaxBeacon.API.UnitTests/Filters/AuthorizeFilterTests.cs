using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Filters;

namespace TaxBeacon.API.UnitTests.Filters;

public class AuthorizeFilterTests
{
    private readonly AuthorizeFilter _authorizeFilter;

    public AuthorizeFilterTests()
    {
        Mock<ILogger<AuthorizeFilter>> loggerMock = new();
        _authorizeFilter = new AuthorizeFilter(loggerMock.Object);
    }

    [Fact]
    public async Task OnAuthorizationAsync_UserIdExistsInClaims_SuccessfullyAuthorized()
    {
        // Arrange
        var contextUser = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(Claims.UserId, Guid.NewGuid().ToString())
        }));

        var actionDescriptor = new ActionDescriptor
        {
            EndpointMetadata = new List<object> { new AuthorizeAttribute() }
        };

        var httpContext = new DefaultHttpContext { User = contextUser };
        var actionContext = new ActionContext(httpContext, Mock.Of<RouteData>(), actionDescriptor);
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
        var actionDescriptor = new ActionDescriptor
        {
            EndpointMetadata = new List<object> { new AuthorizeAttribute() }
        };

        var httpContext = new DefaultHttpContext { User = contextUser };
        var actionContext = new ActionContext(httpContext, Mock.Of<RouteData>(), actionDescriptor);
        var authorizationFilterContext = new AuthorizationFilterContext(
            actionContext, new List<IFilterMetadata>());

        // Act
        await _authorizeFilter.OnAuthorizationAsync(authorizationFilterContext);

        // Assert
        authorizationFilterContext.Result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task OnAuthorizationAsync_EndpointWithoutAuthorizeAttribute_ReturnsResultWithoutErrors()
    {
        // Arrange
        var contextUser = new ClaimsPrincipal(new ClaimsIdentity());
        var actionDescriptor = new ActionDescriptor { EndpointMetadata = Enumerable.Empty<object>().ToList() };

        var httpContext = new DefaultHttpContext { User = contextUser };
        var actionContext = new ActionContext(httpContext, Mock.Of<RouteData>(), actionDescriptor);
        var authorizationFilterContext = new AuthorizationFilterContext(
            actionContext, new List<IFilterMetadata>());

        // Act
        await _authorizeFilter.OnAuthorizationAsync(authorizationFilterContext);

        // Assert
        authorizationFilterContext.Result.Should().BeNull();
    }

    [Fact]
    public async Task OnAuthorizationAsync_EndpointWithAllowAnonymousAttribute_ReturnsResultWithoutErrors()
    {
        // Arrange
        var contextUser = new ClaimsPrincipal(new ClaimsIdentity());
        var actionDescriptor = new ActionDescriptor
        {
            EndpointMetadata = new List<object> { new AuthorizeAttribute(), new AllowAnonymousAttribute() }
        };

        var httpContext = new DefaultHttpContext { User = contextUser };
        var actionContext = new ActionContext(httpContext, Mock.Of<RouteData>(), actionDescriptor);
        var authorizationFilterContext = new AuthorizationFilterContext(
            actionContext, new List<IFilterMetadata>());

        // Act
        await _authorizeFilter.OnAuthorizationAsync(authorizationFilterContext);

        // Assert
        authorizationFilterContext.Result.Should().BeNull();
    }
}
