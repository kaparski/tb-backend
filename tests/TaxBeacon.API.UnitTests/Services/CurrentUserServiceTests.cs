using Bogus;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Security.Claims;
using System.Security.Principal;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Services;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL;
using TaxBeacon.DAL.Entities;
using TaxBeacon.DAL.Interceptors;
using TaxBeacon.DAL.Interfaces;
using RolesConstants = TaxBeacon.Common.Roles.Roles;

namespace TaxBeacon.API.UnitTests.Services;

public class CurrentUserServiceTests
{
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly Mock<EntitySaveChangesInterceptor> _entitySaveChangesInterceptorMock;
    private readonly ITaxBeaconDbContext _dbContextMock;
    private readonly ICurrentUserService _currentUserService;

    public CurrentUserServiceTests()
    {
        _httpContextAccessorMock = new();
        _entitySaveChangesInterceptorMock = new();

        _dbContextMock = new TaxBeaconDbContext(
            new DbContextOptionsBuilder<TaxBeaconDbContext>()
                .UseInMemoryDatabase($"{nameof(CurrentUserServiceTests)}-InMemoryDb-{Guid.NewGuid()}")
                .Options,
            _entitySaveChangesInterceptorMock.Object);

        _currentUserService = new CurrentUserService(_httpContextAccessorMock.Object);
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

    [Fact]
    public void UserInfo_UserNotFoundInDb_Throws()
    {
        // Act
        var act = () => { var actualResult = _currentUserService.UserInfo; };

        // Assert
        using (new AssertionScope())
        {
            act.Should()
                .Throw<InvalidOperationException>();
        }
    }

    [Fact]
    public void UserInfo_ExistingUser_Succeeds()
    {
        // Arrange
        var user = TestData.TestUser.Generate();
        var rolesNames = TestData.TestRole.Generate(2)
            .OrderBy(r => r.Name)
            .Select(r => r.Name)
            .ToList();

        var identity = new GenericIdentity("test", "test");
        rolesNames.ForEach(roleName => identity.AddClaim(new Claim(Claims.TenantRoles, roleName)));
        identity.AddClaim(new Claim(Claims.FullName, user.FullName));
        var contextUser = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = contextUser };

        _httpContextAccessorMock
            .Setup(contextAccessor => contextAccessor.HttpContext)
            .Returns(httpContext);

        // Act

        var actualResult = _currentUserService.UserInfo;

        // Assert
        using (new AssertionScope())
        {
            actualResult.FullName.Should().Be(user.FullName);
            actualResult.Roles.Should().Be(string.Join(", ", rolesNames));
        }
    }

    [Fact]
    public void IsSuperAdmin_ContainsSuperAdminRole_ReturnsTrue()
    {
        // Arrange

        var identity = new GenericIdentity("test", "test");
        identity.AddClaim(new Claim(Claims.Roles, RolesConstants.SuperAdmin));
        var contextUser = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = contextUser };

        _httpContextAccessorMock
            .Setup(contextAccessor => contextAccessor.HttpContext)
            .Returns(httpContext);

        // Act

        var actualResult = _currentUserService.IsSuperAdmin;

        // Assert

        actualResult.Should().Be(true);
    }

    [Fact]
    public void IsSuperAdmin_DoesNotContainSuperAdminRole_ReturnsFalse()
    {
        // Arrange

        var identity = new GenericIdentity("test", "test");
        var contextUser = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = contextUser };

        _httpContextAccessorMock
            .Setup(contextAccessor => contextAccessor.HttpContext)
            .Returns(httpContext);

        // Act

        var actualResult = _currentUserService.IsSuperAdmin;

        // Assert

        actualResult.Should().Be(false);
    }

    private static class TestData
    {
        public static readonly Faker<User> TestUser =
            new Faker<User>()
                .RuleFor(u => u.Id, _ => Guid.NewGuid())
                .RuleFor(u => u.FirstName, f => f.Name.FirstName())
                .RuleFor(u => u.LastName, f => f.Name.LastName())
                .RuleFor(u => u.FullName, (_, u) => $"{u.FirstName} {u.LastName}")
                .RuleFor(u => u.LegalName, (_, u) => u.FirstName)
                .RuleFor(u => u.Email, f => f.Internet.Email())
                .RuleFor(u => u.CreatedDateTimeUtc, _ => DateTime.UtcNow)
                .RuleFor(u => u.Status, f => f.PickRandom<Status>());

        public static readonly Faker<Tenant> TestTenant =
            new Faker<Tenant>()
                .RuleFor(t => t.Id, _ => Guid.NewGuid())
                .RuleFor(t => t.Name, f => f.Company.CompanyName())
                .RuleFor(t => t.CreatedDateTimeUtc, _ => DateTime.UtcNow);

        public static readonly Faker<Role> TestRole =
            new Faker<Role>()
                .RuleFor(u => u.Id, _ => Guid.NewGuid())
                .RuleFor(u => u.Name, f => f.Name.JobTitle());
    }
}
