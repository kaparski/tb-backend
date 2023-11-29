using FluentAssertions.Execution;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace TaxBeacon.Administration.UnitTests.Services.Roles;
public partial class RoleServiceTests
{
    [Fact]
    public async Task UnassignUsersAsync_UserInTenantAndUserIds_ReturnsSuccess()
    {
        //Arrange
        await TestData.SeedTenantRolesAsync(_dbContextMock, 1, 3);

        var users = await _dbContextMock.Users.ToListAsync();
        var tenant = await _dbContextMock.Tenants.FirstAsync();
        var role = await _dbContextMock.Roles.FirstAsync();

        _currentUserServiceMock
            .Setup(x => x.TenantId)
            .Returns(tenant.Id);
        _currentUserServiceMock
            .Setup(x => x.IsSuperAdmin)
            .Returns(false);

        //Act
        var actualResult = await _roleService.UnassignUsersAsync(role.Id, new List<Guid>
        {
            users[0].Id, users[1].Id
        });

        //Assert
        using (new AssertionScope())
        {
            actualResult.IsT0.Should().BeTrue();
            _dbContextMock.TenantUserRoles.Count().Should().Be(1);
            _dbContextMock.UserActivityLogs.Count().Should().Be(2);
        }
    }

    [Fact]
    public async Task UnassignUsersAsync_SuperAdminWithoutTenantAndUserIds_ReturnsSuccess()
    {
        //Arrange
        await TestData.SeedNotTenantRolesAsync(_dbContextMock, 1, 3);

        var users = await _dbContextMock.Users.ToListAsync();
        var role = await _dbContextMock.Roles.FirstAsync();

        _currentUserServiceMock
            .Setup(x => x.TenantId)
            .Returns(Guid.Empty);
        _currentUserServiceMock
            .Setup(x => x.IsSuperAdmin)
            .Returns(true);

        //Act
        var actualResult = await _roleService.UnassignUsersAsync(role.Id, new List<Guid>
        {
            users[0].Id, users[1].Id
        });

        //Assert
        using (new AssertionScope())
        {
            actualResult.IsT0.Should().BeTrue();
            _dbContextMock.UserRoles.Count().Should().Be(1);
            _dbContextMock.UserActivityLogs.Count().Should().Be(2);
        }
    }

    [Fact]
    public async Task UnassignUsersAsync_IncorrectRoleIdAndUserInTenant_ReturnsNotFound()
    {
        //Arrange
        await TestData.SeedTenantRolesAsync(_dbContextMock, 1, 3);
        var tenant = await _dbContextMock.Tenants.FirstAsync();

        _currentUserServiceMock
            .Setup(x => x.TenantId)
            .Returns(tenant.Id);
        _currentUserServiceMock
            .Setup(x => x.IsSuperAdmin)
            .Returns(false);

        //Act
        var result = await _roleService.UnassignUsersAsync(new Guid(), new List<Guid>());

        //Assert
        result.IsT1.Should().BeTrue();
    }

    [Fact]
    public async Task UnassignUsersAsync_IncorrectRoleIdAndSuperAdminWithoutTenant_ReturnsNotFound()
    {
        //Arrange
        await TestData.SeedNotTenantRolesAsync(_dbContextMock, 1, 3);

        _currentUserServiceMock
            .Setup(x => x.TenantId)
            .Returns(Guid.Empty);
        _currentUserServiceMock
            .Setup(x => x.IsSuperAdmin)
            .Returns(true);

        //Act
        var result = await _roleService.UnassignUsersAsync(new Guid(), new List<Guid>());

        //Assert
        result.IsT1.Should().BeTrue();
    }
}
