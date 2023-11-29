using Bogus;
using FluentAssertions.Execution;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TaxBeacon.DAL.Administration.Entities;

namespace TaxBeacon.Administration.UnitTests.Services.Roles;
public partial class RoleServiceTests
{
    [Fact]
    public async Task GetRolePermissionsByIdAsync_ExistingRoleIdAndUserInTenant_ReturnsRolePermissions()
    {
        // Arrange
        var roles = await TestData.SeedTenantRolesAsync(_dbContextMock, 1, 3);
        var tenant = await _dbContextMock.Tenants.FirstAsync();
        var permissions = new Faker().Random
            .WordsArray(4)
            .Select(word => new Permission
            {
                Id = Guid.NewGuid(),
                Name = word
            })
            .ToList();

        await _dbContextMock.TenantPermissions.AddRangeAsync(permissions
            .Select(permission => new TenantPermission
            {
                Tenant = tenant,
                Permission = permission
            }));

        await _dbContextMock.TenantRolePermissions.AddRangeAsync(permissions
            .Select(permission => new TenantRolePermission
            {
                TenantId = tenant.Id,
                RoleId = roles[0].Id,
                PermissionId = permission.Id
            }));
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(service => service.TenantId)
            .Returns(tenant.Id);
        _currentUserServiceMock
            .Setup(service => service.IsSuperAdmin)
            .Returns(false);

        // Act
        var actualResult = await _roleService.GetRolePermissionsByIdAsync(roles[0].Id);

        // Assert
        using (new AssertionScope())
        {
            actualResult.TryPickT0(out var actualPermissions, out _).Should().BeTrue();
            actualPermissions.Select(p => p.Id).Should().BeEquivalentTo(permissions.Select(perm => perm.Id));
        }
    }

    [Fact]
    public async Task GetRolePermissionsByIdAsync_ExistingRoleIdAndSuperAdminWithoutTenant_ReturnsRolePermissions()
    {
        // Arrange
        var roles = await TestData.SeedNotTenantRolesAsync(_dbContextMock, 1, 3);
        var permissions = new Faker().Random
            .WordsArray(4)
            .Select(word => new Permission
            {
                Id = Guid.NewGuid(),
                Name = word
            }).ToList();

        await _dbContextMock.RolePermissions.AddRangeAsync(permissions
            .Select(permission => new RolePermission
            {
                Role = roles[0],
                Permission = permission
            }));
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(service => service.TenantId)
            .Returns(Guid.Empty);
        _currentUserServiceMock
            .Setup(service => service.IsSuperAdmin)
            .Returns(true);

        // Act
        var actualResult = await _roleService.GetRolePermissionsByIdAsync(roles[0].Id);

        // Assert
        using (new AssertionScope())
        {
            actualResult.TryPickT0(out var actualPermissions, out _).Should().BeTrue();
            actualPermissions.Select(p => p.Id).Should().BeEquivalentTo(permissions.Select(perm => perm.Id));
        }
    }

    [Fact]
    public async Task GetRolePermissionsByIdAsync_NotExistingRoleIdAndUserInTenant_ReturnsNotFound()
    {
        // Arrange
        _currentUserServiceMock
            .Setup(service => service.TenantId)
            .Returns(Guid.NewGuid());
        _currentUserServiceMock
            .Setup(service => service.IsSuperAdmin)
            .Returns(false);

        // Act
        var actualResult = await _roleService.GetRolePermissionsByIdAsync(Guid.NewGuid());

        // Assert
        actualResult.IsT1.Should().BeTrue();
    }

    [Fact]
    public async Task GetRolePermissionsByIdAsync_NonExistingRoleIdAndSuperAdminWithoutTenant_ReturnsNotFound()
    {
        // Arrange
        _currentUserServiceMock
            .Setup(service => service.TenantId)
            .Returns(Guid.Empty);
        _currentUserServiceMock
            .Setup(service => service.IsSuperAdmin)
            .Returns(true);

        // Act
        var actualResult = await _roleService.GetRolePermissionsByIdAsync(Guid.NewGuid());

        // Assert
        actualResult.IsT1.Should().BeTrue();
    }
}
