using FluentAssertions;
using TaxBeacon.DAL.Administration.Entities;

namespace TaxBeacon.Administration.UnitTests.Services.Users;

public partial class UserServiceTests
{
    [Fact]
    public async Task AssignRoleAsync_ValidRoleIds_ShouldAssignAllProvidedRoles()
    {
        //Arrange
        _dateTimeServiceMock.SetupSequence(x => x.UtcNow)
            .Returns(DateTime.UtcNow)
            .Returns(DateTime.UtcNow);

        var tenant = TestData.TenantFaker.Generate();
        var user = TestData.UserFaker.Generate();
        var roles = TestData.RoleFaker.Generate(2);
        var tenantUser = new TenantUser { Tenant = tenant, User = user };

        user.TenantUsers.Add(tenantUser);
        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Users.AddAsync(user);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(service => service.TenantId)
            .Returns(tenant.Id);

        _currentUserServiceMock
            .Setup(service => service.IsSuperAdmin)
            .Returns(false);

        //Act
        await _userService.ChangeUserRolesAsync(user.Id, roles.Select(x => x.Id).ToArray());

        //Assert
        _dbContextMock.TenantUserRoles.Count().Should().Be(2);
    }

    [Fact]
    public async Task AssignRoleAsync_ValidRoleIds_ShouldAssignAllProvidedRolesAndRemoveNotAssigned()
    {
        //Arrange
        _dateTimeServiceMock.SetupSequence(x => x.UtcNow)
            .Returns(DateTime.UtcNow)
            .Returns(DateTime.UtcNow);

        var tenant = TestData.TenantFaker.Generate();
        var user = TestData.UserFaker.Generate();
        var roles = TestData.RoleFaker.Generate(2);
        var tenantRoles = roles.Select(x => new TenantRole { Tenant = tenant, Role = x });
        var tenantUser = new TenantUser { Tenant = tenant, User = user };

        foreach (var tenantRole in tenantRoles)
        {
            await _dbContextMock.TenantUserRoles.AddAsync(new TenantUserRole
            {
                TenantUser = tenantUser,
                TenantRole = tenantRole
            });
        }

        user.TenantUsers.Add(tenantUser);
        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Users.AddAsync(user);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(service => service.TenantId)
            .Returns(tenant.Id);

        _currentUserServiceMock
            .Setup(service => service.IsSuperAdmin)
            .Returns(false);

        //Act
        await _userService.ChangeUserRolesAsync(user.Id, new[] { roles.Select(x => x.Id).First() });

        //Assert
        _dbContextMock.TenantUserRoles.Count().Should().Be(1);
    }
}
