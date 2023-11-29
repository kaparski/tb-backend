using FluentAssertions;
using FluentAssertions.Execution;
using OneOf.Types;

namespace TaxBeacon.Administration.UnitTests.Services.Users;

public partial class UserServiceTests
{
    [Fact]
    public async Task GetUserProfileAsync_UserNotInTenantAndUserExists_ReturnsUserDto()
    {
        // Arrange
        var userViews = TestData.UserViewFaker.Generate(5);
        var expectedUser = userViews.First();

        await _dbContextMock.UsersView.AddRangeAsync(userViews);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(service => service.IsSuperAdmin)
            .Returns(false);

        _currentUserServiceMock
            .Setup(service => service.IsUserInTenant)
            .Returns(false);

        _currentUserServiceMock
            .Setup(service => service.UserId)
            .Returns(expectedUser.Id);

        // Act
        var actualResult = await _userService.GetUserProfileAsync();

        // Assert

        using (new AssertionScope())
        {
            actualResult.TryPickT0(out var actualUserDto, out _).Should().BeTrue();
            actualUserDto.Should().NotBeNull()
                .And
                .BeEquivalentTo(expectedUser, opt => opt.ExcludingMissingMembers());
        }
    }

    [Fact]
    public async Task GetUserProfileAsync_UserNotInTenantAndUserDoesntExist_ReturnsNotFound()
    {
        // Arrange
        var userViews = TestData.UserViewFaker.Generate(5);

        await _dbContextMock.UsersView.AddRangeAsync(userViews);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(service => service.IsSuperAdmin)
            .Returns(false);

        _currentUserServiceMock
            .Setup(service => service.IsUserInTenant)
            .Returns(false);

        _currentUserServiceMock
            .Setup(service => service.UserId)
            .Returns(Guid.NewGuid());

        // Act
        var actualResult = await _userService.GetUserProfileAsync();

        // Assert

        using (new AssertionScope())
        {
            actualResult.TryPickT1(out var notFound, out _).Should().BeTrue();
            notFound.Should().BeOfType<NotFound>();
        }
    }

    [Fact]
    public async Task GetUserProfileAsync_UserInTenantAndUserExists_ReturnsUserDto()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        var tenantUserViews = TestData.TenantUserViewFaker
            .RuleFor(u => u.TenantId, _ => tenant.Id)
            .Generate(5);

        var expectedUser = tenantUserViews.First();

        await _dbContextMock.Tenants.AddRangeAsync(tenant);
        await _dbContextMock.TenantUsersView.AddRangeAsync(tenantUserViews);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(service => service.IsSuperAdmin)
            .Returns(false);

        _currentUserServiceMock
            .SetupGet(service => service.TenantId)
            .Returns(tenant.Id);

        _currentUserServiceMock
            .Setup(service => service.IsUserInTenant)
            .Returns(true);

        _currentUserServiceMock
            .Setup(service => service.UserId)
            .Returns(expectedUser.Id);

        // Act
        var actualResult = await _userService.GetUserProfileAsync();

        // Assert

        using (new AssertionScope())
        {
            actualResult.TryPickT0(out var actualUserDto, out _).Should().BeTrue();
            actualUserDto.Should().NotBeNull()
                .And
                .BeEquivalentTo(expectedUser, opt => opt.ExcludingMissingMembers());
        }
    }

    [Fact]
    public async Task GetUserProfileAsync_UserInAndUserDoesntExist_ReturnsNotFound()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        var tenantUserViews = TestData.TenantUserViewFaker
            .RuleFor(u => u.TenantId, _ => tenant.Id)
            .Generate(5);

        await _dbContextMock.Tenants.AddRangeAsync(tenant);
        await _dbContextMock.TenantUsersView.AddRangeAsync(tenantUserViews);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(service => service.IsSuperAdmin)
            .Returns(false);

        _currentUserServiceMock
            .SetupGet(service => service.TenantId)
            .Returns(tenant.Id);

        _currentUserServiceMock
            .Setup(service => service.IsUserInTenant)
            .Returns(true);

        _currentUserServiceMock
            .Setup(service => service.UserId)
            .Returns(Guid.NewGuid());

        // Act
        var actualResult = await _userService.GetUserProfileAsync();

        // Assert

        using (new AssertionScope())
        {
            actualResult.TryPickT1(out var notFound, out _).Should().BeTrue();
            notFound.Should().BeOfType<NotFound>();
        }
    }

    [Fact]
    public async Task GetUserProfileAsync_SuperAdminInTenantAndUserExists_ReturnsUserDto()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        var userViews = TestData.UserViewFaker.Generate(5);
        var expectedUser = userViews.First();

        await _dbContextMock.Tenants.AddRangeAsync(tenant);
        await _dbContextMock.UsersView.AddRangeAsync(userViews);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(service => service.IsSuperAdmin)
            .Returns(true);

        _currentUserServiceMock
            .Setup(service => service.IsUserInTenant)
            .Returns(true);

        _currentUserServiceMock
            .Setup(service => service.UserId)
            .Returns(expectedUser.Id);

        _currentUserServiceMock
            .SetupGet(service => service.TenantId)
            .Returns(tenant.Id);

        // Act
        var actualResult = await _userService.GetUserProfileAsync();

        // Assert

        using (new AssertionScope())
        {
            actualResult.TryPickT0(out var actualUserDto, out _).Should().BeTrue();
            actualUserDto.Should().NotBeNull()
                .And
                .BeEquivalentTo(expectedUser, opt => opt.ExcludingMissingMembers());
        }
    }

    [Fact]
    public async Task GetUserProfileAsync_SuperAdminNotInTenantAndUserDoesntExist_ReturnsNotFound()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        var userViews = TestData.UserViewFaker.Generate(5);

        await _dbContextMock.Tenants.AddRangeAsync(tenant);
        await _dbContextMock.UsersView.AddRangeAsync(userViews);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(service => service.IsSuperAdmin)
            .Returns(true);

        _currentUserServiceMock
            .Setup(service => service.IsUserInTenant)
            .Returns(true);

        _currentUserServiceMock
            .Setup(service => service.UserId)
            .Returns(Guid.NewGuid());

        _currentUserServiceMock
            .SetupGet(service => service.TenantId)
            .Returns(tenant.Id);

        // Act
        var actualResult = await _userService.GetUserProfileAsync();

        // Assert

        using (new AssertionScope())
        {
            actualResult.TryPickT1(out var notFound, out _).Should().BeTrue();
            notFound.Should().BeOfType<NotFound>();
        }
    }
}
