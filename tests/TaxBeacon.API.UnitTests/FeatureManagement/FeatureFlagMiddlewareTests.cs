using Bogus;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Moq;
using System.Text.Json;
using TaxBeacon.API.Extensions;
using TaxBeacon.API.FeatureManagement;
using TaxBeacon.Common.FeatureManagement;

namespace TaxBeacon.API.UnitTests.FeatureManagement;

public class FeatureFlagMiddlewareTests
{
    private readonly Mock<IWebHostEnvironment> _webHostEnvironmentMock;
    private readonly FeatureFlagMiddleware _featureFlagMiddleware;

    public FeatureFlagMiddlewareTests()
    {
        _webHostEnvironmentMock = new();
        _featureFlagMiddleware = new FeatureFlagMiddleware(
            _ => Task.CompletedTask,
            TestData.TestFeatureFlags,
            _webHostEnvironmentMock.Object);
    }

    [Fact]
    public async Task InvokeAsync_NotDevEnvironment_FeatureFlagsChangedInCookies()
    {
        // Arrange
        var responseCookiesMock = new Mock<IResponseCookies>();
        var requestCookiesMock = new Mock<IRequestCookieCollection>();
        requestCookiesMock
            .Setup(rc => rc[TestData.CookieKey])
            .Returns(string.Empty);

        var httpContextMock = new Mock<HttpContext>();
        httpContextMock
            .SetupGet(hc => hc.Request.Cookies)
            .Returns(requestCookiesMock.Object);
        httpContextMock
            .SetupGet(hc => hc.Response.Cookies)
            .Returns(responseCookiesMock.Object);
        _webHostEnvironmentMock
            .Setup(s => s.EnvironmentName)
            .Returns(Environments.Staging);

        var configurationFeatureFlagsString = JsonSerializer.Serialize(TestData.TestFeatureFlags.Flags,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, });

        // Act
        await _featureFlagMiddleware.InvokeAsync(httpContextMock.Object);

        // Assert
        responseCookiesMock
            .Verify(
                s => s.Append(TestData.CookieKey, configurationFeatureFlagsString, It.IsAny<CookieOptions>()),
                Times.Once);
    }

    [Theory]
    [MemberData(nameof(TestData.DevEnvironments), MemberType = typeof(TestData))]
    public async Task InvokeAsync_FeatureFlagsNotExistInCookiesAndDevEnvironment_FeatureFlagsChangedInCookies(string environment)
    {
        // Arrange
        var responseCookiesMock = new Mock<IResponseCookies>();
        var requestCookiesMock = new Mock<IRequestCookieCollection>();
        requestCookiesMock
            .Setup(rc => rc[TestData.CookieKey])
            .Returns(string.Empty);

        var httpContextMock = new Mock<HttpContext>();
        httpContextMock
            .SetupGet(hc => hc.Request.Cookies)
            .Returns(requestCookiesMock.Object);
        httpContextMock
            .SetupGet(hc => hc.Response.Cookies)
            .Returns(responseCookiesMock.Object);

        _webHostEnvironmentMock
            .Setup(s => s.EnvironmentName)
            .Returns(environment);

        var configurationFeatureFlagsString = JsonSerializer.Serialize(TestData.TestFeatureFlags.Flags,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, });

        // Act
        await _featureFlagMiddleware.InvokeAsync(httpContextMock.Object);

        // Assert
        responseCookiesMock
            .Verify(
                s => s.Append(TestData.CookieKey, configurationFeatureFlagsString, It.IsAny<CookieOptions>()),
                Times.Once);
    }

    [Theory]
    [MemberData(nameof(TestData.DevEnvironments), MemberType = typeof(TestData))]
    public async Task InvokeAsync_FeatureFlagsExistInCookiesAndDevEnvironment_FeatureFlagsChangedInCookies(string environment)
    {
        // Arrange
        var responseCookiesMock = new Mock<IResponseCookies>();
        var requestCookiesMock = new Mock<IRequestCookieCollection>();
        var expectedResult = new Dictionary<string, bool>(TestData.TestFeatureFlags.Flags);
        var firstKey = expectedResult.Keys.First();
        expectedResult[firstKey] = !expectedResult[firstKey];

        var cookiesFeatureFlags = new Dictionary<string, bool>(expectedResult) { { "test", false } };

        requestCookiesMock
            .Setup(rc => rc[TestData.CookieKey])
            .Returns(JsonSerializer.Serialize(cookiesFeatureFlags));

        var httpContextMock = new Mock<HttpContext>();
        httpContextMock
            .SetupGet(hc => hc.Request.Cookies)
            .Returns(requestCookiesMock.Object);
        httpContextMock
            .SetupGet(hc => hc.Response.Cookies)
            .Returns(responseCookiesMock.Object);

        _webHostEnvironmentMock
            .Setup(s => s.EnvironmentName)
            .Returns(environment);

        var expectedFeatureFlagsString = JsonSerializer.Serialize(expectedResult,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, });

        // Act
        await _featureFlagMiddleware.InvokeAsync(httpContextMock.Object);

        // Assert
        responseCookiesMock
            .Verify(
                s => s.Append(TestData.CookieKey, expectedFeatureFlagsString, It.IsAny<CookieOptions>()),
                Times.Once);
    }

    private static class TestData
    {
        public const string CookieKey = $"{nameof(FeatureFlags)}0";

        public static readonly FeatureFlags TestFeatureFlags = new Faker<FeatureFlags>()
            .RuleFor(f => f.Flags,
                f => f.Random.WordsArray(8).Distinct().ToDictionary(key => key, _ => f.Random.Bool()))
            .Generate();

        public static IEnumerable<object[]> DevEnvironments = new List<object[]>
        {
            new object[] { Environments.Development },
            new object[] { EnvironmentExtensions.CloudDevelopment },
        };
    }
}
