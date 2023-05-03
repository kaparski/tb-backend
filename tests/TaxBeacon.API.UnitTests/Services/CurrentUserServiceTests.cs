using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Security.Claims;
using System.Security.Principal;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Services;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL.Interfaces;
using TaxBeacon.UserManagement.Services;

namespace TaxBeacon.API.UnitTests.Services;

public class CurrentUserServiceTests
{
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly Mock<ITaxBeaconDbContext> _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public CurrentUserServiceTests()
    {
        _httpContextAccessorMock = new();
        _dbContext = new();
        _currentUserService = new CurrentUserService(_httpContextAccessorMock.Object, _dbContext.Object);
    }

    [Fact]
    public void UserId_UserWithUserIdClaim_ReturnsUserId()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var identity = new GenericIdentity("test", "test");
        identity.AddClaim(new Claim(Claims.UserIdClaimName, userId.ToString()));
        var contextUser = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = contextUser };

        _httpContextAccessorMock
            .Setup(contextAccessor => contextAccessor.HttpContext)
            .Returns(httpContext);

        // Act
        var actualResult = _currentUserService.UserId;

        // Assert
        actualResult.Should().NotBeEmpty();
        actualResult.Should().Be(userId);
    }

    [Fact]
    public void UserId_UserWithoutUserIdClaim_ReturnGuidEmpty()
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

    [Fact]
    public void TenantId_UserWithTenantIdClaim_ReturnsTenantId()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var identity = new GenericIdentity("test", "test");
        identity.AddClaim(new Claim(Claims.TenantId, tenantId.ToString()));
        var contextUser = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = contextUser };

        _httpContextAccessorMock
            .Setup(contextAccessor => contextAccessor.HttpContext)
            .Returns(httpContext);

        // Act
        var actualResult = _currentUserService.TenantId;

        // Assert
        actualResult.Should().NotBeEmpty();
        actualResult.Should().Be(tenantId);
    }

    [Fact]
    public void TenantId_UserWithoutTenantIdClaim_ReturnsGuidEmpty()
    {
        // Arrange
        var identity = new GenericIdentity("test", "test");
        var contextUser = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = contextUser };

        _httpContextAccessorMock
            .Setup(contextAccessor => contextAccessor.HttpContext)
            .Returns(httpContext);

        // Act
        var actualResult = _currentUserService.TenantId;

        // Assert
        actualResult.Should().BeEmpty();
    }
}
