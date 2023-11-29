using FluentAssertions.Execution;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TaxBeacon.DAL.Administration.Entities;

namespace TaxBeacon.Administration.UnitTests.Services.Roles;
public partial class RoleServiceTests
{
    [Fact]
    public async Task AssignUsersAsync_ExistingRoleIdAndNewUsersAndUserInTenant_ReturnsSuccess()
    {
        // Arrange
        var roles = await TestData.SeedTenantRolesAsync(_dbContextMock, 1, 3);
        var tenant = await _dbContextMock.Tenants.FirstAsync();
        var usersToAssign = TestData.UserFaker.Generate(3);

        await _dbContextMock.Users.AddRangeAsync(usersToAssign);
        await _dbContextMock.TenantUsers.AddRangeAsync(usersToAssign.Select(u => new TenantUser
        {
            TenantId = tenant.Id,
            UserId = u.Id
        }));
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(x => x.TenantId)
            .Returns(tenant.Id);
        _currentUserServiceMock
            .Setup(x => x.IsSuperAdmin)
            .Returns(false);

        // Act
        var result = await _roleService.AssignUsersAsync(
            roles[0].Id, usersToAssign.Select(x => x.Id).ToList());

        // Assert
        using (new AssertionScope())
        {
            result.IsT0.Should().BeTrue();
            _dbContextMock.TenantUserRoles.Count().Should().Be(6);
            _dbContextMock.UserRoles.Count().Should().Be(0);
            _dbContextMock.UserActivityLogs.Count().Should().Be(3);
        }
    }

    [Fact]
    public async Task AssignUsersAsync_ExistingRoleIdAndNewUsersAndSuperAdminWithoutTenant_ReturnsSuccess()
    {
        // Arrange
        var roles = await TestData.SeedNotTenantRolesAsync(_dbContextMock, 1, 3);
        var usersToAssign = TestData.UserFaker.Generate(3);

        await _dbContextMock.Users.AddRangeAsync(usersToAssign);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(x => x.TenantId)
            .Returns(Guid.Empty);
        _currentUserServiceMock
            .Setup(x => x.IsSuperAdmin)
            .Returns(true);

        // Act
        var result = await _roleService.AssignUsersAsync(
            roles[0].Id, usersToAssign.Select(x => x.Id).ToList());

        // Assert
        using (new AssertionScope())
        {
            result.IsT0.Should().BeTrue();
            _dbContextMock.UserRoles.Count().Should().Be(6);
            _dbContextMock.TenantUserRoles.Count().Should().Be(0);
            _dbContextMock.UserActivityLogs.Count().Should().Be(3);
        }
    }

    [Fact]
    public async Task AssignUsersAsync_NonExistingRoleIdAndUserInTenant_ShouldReturnNotFound()
    {
        // Arrange
        await TestData.SeedTenantRolesAsync(_dbContextMock, 1, 3);
        var tenant = await _dbContextMock.Tenants.FirstAsync();
        var usersToAssign = TestData.UserFaker.Generate(3);

        await _dbContextMock.Users.AddRangeAsync(usersToAssign);
        await _dbContextMock.TenantUsers.AddRangeAsync(usersToAssign.Select(u => new TenantUser
        {
            TenantId = tenant.Id,
            UserId = u.Id
        }));
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(x => x.TenantId)
            .Returns(tenant.Id);
        _currentUserServiceMock
            .Setup(x => x.IsSuperAdmin)
            .Returns(false);

        // Act
        var result = await _roleService.AssignUsersAsync(
            Guid.NewGuid(), usersToAssign.Select(x => x.Id).ToList());

        // Assert
        using (new AssertionScope())
        {
            result.IsT1.Should().BeTrue();
            _dbContextMock.TenantUserRoles.Count().Should().Be(3);
            _dbContextMock.UserRoles.Count().Should().Be(0);
            _dbContextMock.UserActivityLogs.Count().Should().Be(0);
        }
    }

    [Fact]
    public async Task AssignUsersAsync_NonExistingRoleIdAndSuperAdminWithoutTenant_ShouldReturnNotFound()
    {
        // Arrange
        await TestData.SeedNotTenantRolesAsync(_dbContextMock, 1, 3);
        var usersToAssign = TestData.UserFaker.Generate(3);

        await _dbContextMock.Users.AddRangeAsync(usersToAssign);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(x => x.TenantId)
            .Returns(Guid.Empty);
        _currentUserServiceMock
            .Setup(x => x.IsSuperAdmin)
            .Returns(true);

        // Act
        var result = await _roleService.AssignUsersAsync(
            Guid.NewGuid(), usersToAssign.Select(x => x.Id).ToList());

        // Assert
        using (new AssertionScope())
        {
            result.IsT1.Should().BeTrue();
            _dbContextMock.UserRoles.Count().Should().Be(3);
            _dbContextMock.TenantUserRoles.Count().Should().Be(0);
            _dbContextMock.UserActivityLogs.Count().Should().Be(0);
        }
    }
}
