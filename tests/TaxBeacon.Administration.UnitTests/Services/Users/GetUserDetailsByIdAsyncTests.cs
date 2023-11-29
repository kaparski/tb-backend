using FluentAssertions;
using FluentAssertions.Execution;
using OneOf.Types;

namespace TaxBeacon.Administration.UnitTests.Services.Users;

public partial class UserServiceTests
{
    [Fact]
    public async Task GetUserDetailsByIdAsync_AdminNotInTenantAndUserExists_ReturnsUserDto()
    {
        // Arrange
        var userViews = TestData.UserViewFaker.Generate(5);

        await _dbContextMock.UsersView.AddRangeAsync(userViews);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(service => service.IsSuperAdmin)
            .Returns(true);

        _currentUserServiceMock
            .Setup(service => service.IsUserInTenant)
            .Returns(false);

        var expectedUser = userViews.First();

        // Act
        var actualResult = await _userService.GetUserDetailsByIdAsync(expectedUser.Id);

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
    public async Task GetUserDetailsByIdAsync_AdminNotInTenantAndUserDoesntExist_ReturnsNotFound()
    {
        // Arrange
        var userViews = TestData.UserViewFaker.Generate(5);

        await _dbContextMock.UsersView.AddRangeAsync(userViews);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(service => service.IsSuperAdmin)
            .Returns(true);

        _currentUserServiceMock
            .Setup(service => service.IsUserInTenant)
            .Returns(false);

        // Act
        var actualResult = await _userService.GetUserDetailsByIdAsync(Guid.NewGuid());

        // Assert

        using (new AssertionScope())
        {
            actualResult.TryPickT1(out var notFound, out _).Should().BeTrue();
            notFound.Should().BeOfType<NotFound>();
        }
    }

    [Fact]
    public async Task GetUserDetailsByIdAsync_AdminInTenantAndUserExists_ReturnsUserDto()
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
            .Returns(true);

        _currentUserServiceMock
            .SetupGet(service => service.TenantId)
            .Returns(tenant.Id);

        _currentUserServiceMock
            .Setup(service => service.IsUserInTenant)
            .Returns(true);

        var expectedUser = tenantUserViews.First();

        // Act
        var actualResult = await _userService.GetUserDetailsByIdAsync(expectedUser.Id);

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
    public async Task GetUserDetailsByIdAsync_AdminInTenantAndUserDoesntExist_ReturnsNotFound()
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
            .Returns(true);

        _currentUserServiceMock
            .SetupGet(service => service.TenantId)
            .Returns(tenant.Id);

        _currentUserServiceMock
            .Setup(service => service.IsUserInTenant)
            .Returns(true);

        // Act
        var actualResult = await _userService.GetUserDetailsByIdAsync(Guid.NewGuid());

        // Assert

        using (new AssertionScope())
        {
            actualResult.TryPickT1(out var notFound, out _).Should().BeTrue();
            notFound.Should().BeOfType<NotFound>();
        }
    }
}
