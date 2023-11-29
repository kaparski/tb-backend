using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using TaxBeacon.API.FeatureManagement;
using TaxBeacon.Common.FeatureManagement;

namespace TaxBeacon.API.UnitTests.FeatureManagement;

public sealed class FeatureFlagsServiceTests
{
    [Fact]
    public void GetCurrentFeatureFlags_FeatureFlagsExistInCookies_ReturnsFeatureFlagsFromCookies()
    {
        // Arrange
        var featureFlagsMock = new FeatureFlags
        {
            Flags = new Dictionary<string, bool> { { TestData.FirstFeatureName, true } }
        };

        var expectedResult = new Dictionary<string, bool> { { TestData.SecondFeatureName, true } };

        var requestCookiesMock = new Mock<IRequestCookieCollection>();
        requestCookiesMock
            .Setup(rc => rc[TestData.CookieKey])
            .Returns(JsonSerializer.Serialize(expectedResult,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, }));

        var httpContextMock = new Mock<HttpContext>();
        httpContextMock
            .SetupGet(hc => hc.Request.Cookies)
            .Returns(requestCookiesMock.Object);

        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        httpContextAccessorMock
            .Setup(hca => hca.HttpContext)
            .Returns(httpContextMock.Object);

        var service = new FeatureFlagsService(httpContextAccessorMock.Object, featureFlagsMock);

        // Act
        var actualResult = service.GetCurrentFeatureFlags();

        // Assert
        actualResult.Should().BeEquivalentTo(expectedResult);
    }

    [Fact]
    public void GetCurrentFeatureFlags_FeatureFlagsNotExistInCookies_ReturnsFeatureFlagsFromConfiguration()
    {
        // Arrange
        var expectedResult = new FeatureFlags
        {
            Flags = new Dictionary<string, bool> { { TestData.FirstFeatureName, true } }
        };

        var requestCookiesMock = new Mock<IRequestCookieCollection>();
        requestCookiesMock
            .Setup(rc => rc[TestData.CookieKey])
            .Returns(string.Empty);

        var httpContextMock = new Mock<HttpContext>();
        httpContextMock
            .SetupGet(hc => hc.Request.Cookies)
            .Returns(requestCookiesMock.Object);

        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        httpContextAccessorMock
            .Setup(hca => hca.HttpContext)
            .Returns(httpContextMock.Object);

        var service = new FeatureFlagsService(httpContextAccessorMock.Object, expectedResult);

        // Act
        var actualResult = service.GetCurrentFeatureFlags();

        // Assert
        actualResult.Should().BeEquivalentTo(expectedResult.Flags);
    }

    [Fact]
    public void GetCurrentFeatureFlags_MethodCalledSecondTime_ReturnsSameFeatureFlagsAsInPreviousCall()
    {
        // Arrange
        var expectedResult = new FeatureFlags
        {
            Flags = new Dictionary<string, bool> { { TestData.FirstFeatureName, true } }
        };

        var requestCookiesMock = new Mock<IRequestCookieCollection>();
        requestCookiesMock
            .Setup(rc => rc[TestData.CookieKey])
            .Returns(string.Empty);

        var httpContextMock = new Mock<HttpContext>();
        httpContextMock
            .SetupGet(hc => hc.Request.Cookies)
            .Returns(requestCookiesMock.Object);

        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        httpContextAccessorMock
            .Setup(hca => hca.HttpContext)
            .Returns(httpContextMock.Object);

        var service = new FeatureFlagsService(httpContextAccessorMock.Object, expectedResult);

        // Act
        var firstCallResult = service.GetCurrentFeatureFlags();
        var secondCallResult = service.GetCurrentFeatureFlags();

        // Assert
        secondCallResult
            .Should().BeEquivalentTo(firstCallResult)
            .And.BeEquivalentTo(expectedResult.Flags);
    }

    [Fact]
    public void IsEnabled_FlagKeyExistsAndEnabled_ReturnsTrue()
    {
        // Arrange
        var featureFlagsMock = new FeatureFlags
        {
            Flags = new Dictionary<string, bool> { { TestData.FirstFeatureName, true } }
        };

        var requestCookiesMock = new Mock<IRequestCookieCollection>();
        requestCookiesMock
            .Setup(rc => rc[TestData.CookieKey])
            .Returns(string.Empty);

        var httpContextMock = new Mock<HttpContext>();
        httpContextMock
            .SetupGet(hc => hc.Request.Cookies)
            .Returns(requestCookiesMock.Object);

        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        httpContextAccessorMock
            .Setup(hca => hca.HttpContext)
            .Returns(httpContextMock.Object);

        var service = new FeatureFlagsService(httpContextAccessorMock.Object, featureFlagsMock);

        // Act
        var actualResult = service.IsEnabled(TestData.FirstFeatureName);

        // Assert
        actualResult.Should().BeTrue();
    }

    [Fact]
    public void IsEnabled_FlagKeyExistsAndDisabled_ReturnsFalse()
    {
        // Arrange
        var featureFlagsMock = new FeatureFlags
        {
            Flags = new Dictionary<string, bool> { { TestData.FirstFeatureName, false } }
        };

        var requestCookiesMock = new Mock<IRequestCookieCollection>();
        requestCookiesMock
            .Setup(rc => rc[TestData.CookieKey])
            .Returns(string.Empty);

        var httpContextMock = new Mock<HttpContext>();
        httpContextMock
            .SetupGet(hc => hc.Request.Cookies)
            .Returns(requestCookiesMock.Object);

        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        httpContextAccessorMock
            .Setup(hca => hca.HttpContext)
            .Returns(httpContextMock.Object);

        var service = new FeatureFlagsService(httpContextAccessorMock.Object, featureFlagsMock);

        // Act
        var actualResult = service.IsEnabled(TestData.FirstFeatureName);

        // Assert
        actualResult.Should().BeFalse();
    }

    [Fact]
    public void IsEnabled_FlagKeyNotExists_ReturnsFalse()
    {
        // Arrange
        var featureFlagsMock = new FeatureFlags
        {
            Flags = new Dictionary<string, bool> { { TestData.FirstFeatureName, true } }
        };

        var requestCookiesMock = new Mock<IRequestCookieCollection>();
        requestCookiesMock
            .Setup(rc => rc[TestData.CookieKey])
            .Returns(string.Empty);

        var httpContextMock = new Mock<HttpContext>();
        httpContextMock
            .SetupGet(hc => hc.Request.Cookies)
            .Returns(requestCookiesMock.Object);

        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        httpContextAccessorMock
            .Setup(hca => hca.HttpContext)
            .Returns(httpContextMock.Object);

        var service = new FeatureFlagsService(httpContextAccessorMock.Object, featureFlagsMock);

        // Act
        var actualResult = service.IsEnabled(TestData.SecondFeatureName);

        // Assert
        actualResult.Should().BeFalse();
    }

    [Fact]
    public void UpdateFeatureFlags_AllFlagKeysExist_ReturnsUpdatedFeatureFlags()
    {
        // Arrange
        var configurationFeatureFlags = new FeatureFlags
        {
            Flags = new Dictionary<string, bool>
            {
                { TestData.FirstFeatureName, true }, { TestData.SecondFeatureName, false },
            }
        };

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

        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        httpContextAccessorMock
            .Setup(hca => hca.HttpContext)
            .Returns(httpContextMock.Object);

        var newFeatureFlagsValues = new Dictionary<string, bool>
        {
            { TestData.FirstFeatureName, false }, { TestData.SecondFeatureName, true },
        };

        var expectedResult = new Dictionary<string, bool>
        {
            { TestData.FirstFeatureName, false }, { TestData.SecondFeatureName, true },
        };

        var expectedCookieString = JsonSerializer.Serialize(expectedResult,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, });
        var service = new FeatureFlagsService(httpContextAccessorMock.Object, configurationFeatureFlags);

        // Act
        var actualResult = service.UpdateFeatureFlag(newFeatureFlagsValues);

        // Assert
        using (new AssertionScope())
        {
            actualResult
                .Should().BeEquivalentTo(newFeatureFlagsValues)
                .And.BeEquivalentTo(service.GetCurrentFeatureFlags());

            responseCookiesMock
                .Verify(s => s.Append(TestData.CookieKey, expectedCookieString, It.IsAny<CookieOptions>()),
                    Times.Once);
        }
    }

    [Fact]
    public void UpdateFeatureFlags_SomeFlagKeysNotExist_ReturnsUpdatedFeatureFlags()
    {
        // Arrange
        var configurationFeatureFlags = new FeatureFlags
        {
            Flags = new Dictionary<string, bool> { { TestData.FirstFeatureName, true }, }
        };

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

        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        httpContextAccessorMock
            .Setup(hca => hca.HttpContext)
            .Returns(httpContextMock.Object);

        var newFeatureFlagsValues = new Dictionary<string, bool>
        {
            { TestData.FirstFeatureName, false }, { TestData.SecondFeatureName, true },
        };

        var expectedResult = new Dictionary<string, bool> { { TestData.FirstFeatureName, false } };

        var expectedCookieString = JsonSerializer.Serialize(expectedResult,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, });
        var service = new FeatureFlagsService(httpContextAccessorMock.Object, configurationFeatureFlags);

        // Act
        var actualResult = service.UpdateFeatureFlag(newFeatureFlagsValues);

        // Assert
        using (new AssertionScope())
        {
            actualResult
                .Should().BeEquivalentTo(expectedResult)
                .And.BeEquivalentTo(service.GetCurrentFeatureFlags());

            responseCookiesMock
                .Verify(s => s.Append(TestData.CookieKey, expectedCookieString, It.IsAny<CookieOptions>()),
                    Times.Once);
        }
    }

    [Fact]
    public void ResetFeatureFlags_ReturnsFeatureFlagsFromConfiguration()
    {
        // Arrange
        var configurationFeatureFlags = new FeatureFlags
        {
            Flags = new Dictionary<string, bool> { { TestData.FirstFeatureName, true }, }
        };

        var responseCookiesMock = new Mock<IResponseCookies>();
        var requestCookiesMock = new Mock<IRequestCookieCollection>();
        requestCookiesMock
            .Setup(rc => rc[TestData.CookieKey])
            .Returns(JsonSerializer.Serialize(new Dictionary<string, bool> { { TestData.SecondFeatureName, true } }));

        var httpContextMock = new Mock<HttpContext>();
        httpContextMock
            .SetupGet(hc => hc.Request.Cookies)
            .Returns(requestCookiesMock.Object);
        httpContextMock
            .SetupGet(hc => hc.Response.Cookies)
            .Returns(responseCookiesMock.Object);

        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        httpContextAccessorMock
            .Setup(hca => hca.HttpContext)
            .Returns(httpContextMock.Object);

        var expectedCookieString = JsonSerializer.Serialize(configurationFeatureFlags.Flags,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, });
        var service = new FeatureFlagsService(httpContextAccessorMock.Object, configurationFeatureFlags);

        // Act
        var previousValues = service.GetCurrentFeatureFlags();
        var actualResult = service.ResetFeatureFlags();

        // Assert
        using (new AssertionScope())
        {
            actualResult
                .Should().BeEquivalentTo(configurationFeatureFlags.Flags)
                .And.BeEquivalentTo(service.GetCurrentFeatureFlags())
                .And.NotBeEquivalentTo(previousValues);

            responseCookiesMock
                .Verify(s => s.Append(TestData.CookieKey, expectedCookieString, It.IsAny<CookieOptions>()),
                    Times.Once);
        }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    private static class TestData
    {
        public const string CookieKey = $"{nameof(FeatureFlags)}0";
        public static readonly string FirstFeatureName = nameof(FirstFeatureName);
        public static readonly string SecondFeatureName = nameof(SecondFeatureName);
    }
}
