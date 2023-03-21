using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Security.Claims;
using System.Security.Principal;
using TaxBeacon.API.Services;
using TaxBeacon.Common.Services;

namespace TaxBeacon.API.UnitTests.Services;

public class CurrentUserServiceTests
{
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly ICurrentUserService _currentUserService;

    public CurrentUserServiceTests()
    {
        _httpContextAccessorMock = new();
        _currentUserService = new CurrentUserService(_httpContextAccessorMock.Object);
    }

    [Fact]
    public void UserId_UserWithUserIdClaim_ReturnUserId()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var identity = new GenericIdentity("test", "test");
        identity.AddClaim(new Claim("userId", userId.ToString()));
        var contextUser = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = contextUser };

        _httpContextAccessorMock
            .Setup(contextAccessor => contextAccessor.HttpContext)
            .Returns(httpContext);

        // Act
        var actualResult = _currentUserService.UserId;

        // Assert
        actualResult.ToString().Should().NotBeNullOrEmpty();
        actualResult.ToString().Should().Be(userId.ToString());
    }

    [Fact]
    public void UserId_UserWithoutUserIdClaim_ReturnEmptyString()
    {
        // Arrange
        var identity = new GenericIdentity("test", "test");
        var contextUser = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = contextUser };

        _httpContextAccessorMock
            .Setup(contextAccessor => contextAccessor.HttpContext)
            .Returns(httpContext);

        // Act
        var actualResult = _currentUserService.UserId;

        // Assert
        actualResult.Should().BeEmpty();
    }
}
