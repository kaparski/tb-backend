using FluentAssertions.Execution;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace TaxBeacon.Administration.UnitTests.Services.Roles;
public partial class RoleServiceTests
{
    [Fact]
    public async Task QueryRoleAssignedUsersAsync_ReturnsTenantAssignedUsers()
    {
        //Arrange
        var roles = await TestData.SeedTenantRolesAsync(_dbContextMock);
        var tenantId = (await _dbContextMock.Tenants.FirstAsync()).Id;

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenantId);
        _currentUserServiceMock
            .Setup(s => s.IsSuperAdmin)
            .Returns(false);

        //Act
        var query = await _roleService.QueryRoleAssignedUsersAsync(roles[0].Id);
        var result = query.ToArray();

        //Assert
        using (new AssertionScope())
        {
            result.Should().HaveCount(5);

            foreach (var dto in result)
            {
                var item = _dbContextMock.Users.Single(u => u.Id == dto.Id);

                dto.Should().BeEquivalentTo(item, opt => opt.ExcludingMissingMembers());
            }
        }
    }

    [Fact]
    public async Task QueryRoleAssignedUsersAsync_ReturnsNonTenantAssignedUsers()
    {
        //Arrange
        var roles = await TestData.SeedNotTenantRolesAsync(_dbContextMock);

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(Guid.Empty);
        _currentUserServiceMock
            .Setup(s => s.IsUserInTenant)
            .Returns(false);
        _currentUserServiceMock
            .Setup(s => s.IsSuperAdmin)
            .Returns(true);

        //Act
        var query = await _roleService.QueryRoleAssignedUsersAsync(roles[0].Id);
        var result = query.ToArray();

        //Assert
        using (new AssertionScope())
        {
            result.Should().HaveCount(5);

            foreach (var dto in result)
            {
                var item = _dbContextMock.Users.Single(u => u.Id == dto.Id);

                dto.Should().BeEquivalentTo(item, opt => opt.ExcludingMissingMembers());
            }
        }
    }
}
