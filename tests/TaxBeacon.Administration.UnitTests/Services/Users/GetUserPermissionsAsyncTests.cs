using Bogus;
using FluentAssertions;
using TaxBeacon.DAL.Administration.Entities;

namespace TaxBeacon.Administration.UnitTests.Services.Users;

public partial class UserServiceTests
{
    [Fact]
    public async Task GetUserPermissions_UserIsSuperAdmin_ReturnsNoTenantPermissions()
    {
        // Arrange
        var user = TestData.UserFaker.Generate();
        var permissions = new Faker().Random
            .WordsArray(4)
            .Select(word => new Permission { Id = Guid.NewGuid(), Name = word })
            .ToList();
        var role = new Role { Id = Guid.NewGuid(), Name = new Faker().Random.Word() };

        await _dbContextMock.Users.AddAsync(user);
        await _dbContextMock.Roles.AddAsync(role);
        await _dbContextMock.Permissions.AddRangeAsync(permissions);
        await _dbContextMock.UserRoles.AddAsync(new UserRole { User = user, Role = role });
        await _dbContextMock.RolePermissions.AddRangeAsync(permissions
            .Select(permission => new RolePermission { Role = role, Permission = permission }));
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(service => service.TenantId)
            .Returns(Guid.Empty);

        _currentUserServiceMock
            .Setup(service => service.IsSuperAdmin)
            .Returns(true);

        // Act
        var actualResult = await _userService.GetUserPermissionsAsync(user.Id);

        // Assert
        actualResult.Should().BeEquivalentTo(permissions.Select(permission => permission.Name));
    }

    [Fact]
    public async Task GetUserPermissions_UserIdAndCurrentUserWithTenant_ReturnsTenantPermissions()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        var user = TestData.UserFaker.Generate();
        var permissions = new Faker().Random
            .WordsArray(4)
            .Select(word => new Permission { Id = Guid.NewGuid(), Name = word })
            .ToList();
        var role = new Role { Id = Guid.NewGuid(), Name = new Faker().Random.Word() };

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.TenantUsers.AddAsync(new TenantUser { Tenant = tenant, User = user });
        await _dbContextMock.TenantRoles.AddAsync(new TenantRole { Tenant = tenant, Role = role });
        await _dbContextMock.TenantPermissions.AddRangeAsync(permissions
            .Select(permission => new TenantPermission { Tenant = tenant, Permission = permission }));
        await _dbContextMock.TenantUserRoles.AddAsync(new TenantUserRole
        {
            TenantId = tenant.Id,
            UserId = user.Id,
            RoleId = role.Id
        });
        await _dbContextMock.TenantRolePermissions.AddRangeAsync(permissions
            .Select(permission => new TenantRolePermission
            {
                TenantId = tenant.Id,
                RoleId = role.Id,
                PermissionId = permission.Id
            }));
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(service => service.TenantId)
            .Returns(tenant.Id);

        // Act
        var actualResult = await _userService.GetUserPermissionsAsync(user.Id, tenant.Id);

        // Assert
        actualResult.Should().BeEquivalentTo(permissions.Select(perm => perm.Name));
    }
}
