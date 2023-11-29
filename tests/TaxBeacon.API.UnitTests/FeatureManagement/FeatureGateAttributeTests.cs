using Bogus;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using TaxBeacon.API.FeatureManagement;
using TaxBeacon.Common.FeatureManagement;

namespace TaxBeacon.API.UnitTests.FeatureManagement;

public sealed class FeatureGateAttributeTests
{
    [Fact]
    public void OnActionExecuting_ServiceExistsAndFeatureFlagEnabled_ReturnsSuccessfulContext()
    {
        // Arrange
        var featureFlagKey = new Faker().Name.JobArea();
        var serviceMock = new Mock<IFeatureFlagsService>();
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        serviceMock
            .Setup(s => s.IsEnabled(featureFlagKey))
            .Returns(true);

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IFeatureFlagsService)))
            .Returns(serviceMock.Object);

        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(ILoggerFactory)))
            .Returns(loggerFactoryMock.Object);

        var httpContextMock = new Mock<HttpContext>();
        httpContextMock
            .SetupGet(c => c.RequestServices)
            .Returns(serviceProviderMock.Object);

        var actionContext =
            new ActionContext(httpContextMock.Object, Mock.Of<RouteData>(), Mock.Of<ActionDescriptor>());

        var actionExecutingContext =
            new ActionExecutingContext(actionContext,
                new List<IFilterMetadata>(),
                new Dictionary<string, object?>(),
                Mock.Of<Controller>());

        var featureGateAttribute = new FeatureGateAttribute(featureFlagKey);

        // Act
        featureGateAttribute.OnActionExecuting(actionExecutingContext);

        // Assert
        actionExecutingContext.Result.Should().BeNull();
    }

    [Fact]
    public void OnActionExecuting_ServiceNotExists_ReturnsSuccessfulContext()
    {
        // Arrange
        var featureFlagKey = new Faker().Name.JobArea();
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IFeatureFlagsService)))
            .Returns(null);
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(ILoggerFactory)))
            .Returns(loggerFactoryMock.Object);

        var httpContextMock = new Mock<HttpContext>();
        httpContextMock
            .SetupGet(c => c.RequestServices)
            .Returns(serviceProviderMock.Object);

        var actionContext =
            new ActionContext(httpContextMock.Object, Mock.Of<RouteData>(), Mock.Of<ActionDescriptor>());

        var actionExecutingContext =
            new ActionExecutingContext(actionContext,
                new List<IFilterMetadata>(),
                new Dictionary<string, object?>(),
                Mock.Of<Controller>());

        var featureGateAttribute = new FeatureGateAttribute(featureFlagKey);

        // Act
        featureGateAttribute.OnActionExecuting(actionExecutingContext);

        // Assert
        actionExecutingContext.Result.Should().BeNull();
    }

    [Fact]
    public void OnActionExecuting_ServiceExistsAndFeatureFlagDisabled_ReturnsForbidResult()
    {
        // Arrange
        var featureFlagKey = new Faker().Name.JobArea();
        var serviceMock = new Mock<IFeatureFlagsService>();
        serviceMock
            .Setup(s => s.IsEnabled(featureFlagKey))
            .Returns(false);

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IFeatureFlagsService)))
            .Returns(serviceMock.Object);
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        var logger = new Mock<ILogger<FeatureGateAttribute>>();
        loggerFactoryMock
            .Setup(lf => lf.CreateLogger(It.IsAny<string>()))
            .Returns(logger.Object);
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(ILoggerFactory)))
            .Returns(loggerFactoryMock.Object);

        var httpContextMock = new Mock<HttpContext>();
        httpContextMock
            .SetupGet(c => c.RequestServices)
            .Returns(serviceProviderMock.Object);
        httpContextMock
            .Setup(c => c.User)
            .Returns(new System.Security.Claims.ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(API.Authentication.Claims.UserId, "test") })));

        var actionContext =
            new ActionContext(httpContextMock.Object, Mock.Of<RouteData>(), Mock.Of<ActionDescriptor>());

        var actionExecutingContext =
            new ActionExecutingContext(actionContext,
                new List<IFilterMetadata>(),
                new Dictionary<string, object?>(),
                Mock.Of<Controller>());

        var featureGateAttribute = new FeatureGateAttribute(featureFlagKey);

        // Act
        featureGateAttribute.OnActionExecuting(actionExecutingContext);

        // Assert
        actionExecutingContext.Result.Should().BeOfType<ForbidResult>();
    }
}
