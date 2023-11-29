using FluentAssertions.Execution;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace TaxBeacon.Administration.UnitTests.Services.Roles;
public partial class RoleServiceTests
{
    [Fact]
    public async Task QueryRoles_ReturnsTenantRoles()
    {
        //Arrange
        var items = await TestData.SeedTenantRolesAsync(_dbContextMock, 3, 2);
        var tenantId = (await _dbContextMock.Tenants.FirstAsync()).Id;

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenantId);
        _currentUserServiceMock
            .Setup(s => s.IsUserInTenant)
            .Returns(true);
        _currentUserServiceMock
            .Setup(s => s.IsSuperAdmin)
            .Returns(false);

        // Act
        var query = _roleService.QueryRoles();
        var result = query.ToArray();

        // Assert

        using (new AssertionScope())
        {
            result.Should().HaveCount(3);

            foreach (var dto in result)
            {
                var item = items.Single(u => u.Id == dto.Id);

                dto.Should().BeEquivalentTo(item, opt => opt.ExcludingMissingMembers());

                dto.AssignedUsersCount.Should().Be(2);
            }
        }
    }

    [Fact]
    public async Task QueryRoles_ReturnsNonTenantRoles()
    {
        //Arrange
        var items = await TestData.SeedNotTenantRolesAsync(_dbContextMock, 3, 2);

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(Guid.Empty);
        _currentUserServiceMock
            .Setup(s => s.IsUserInTenant)
            .Returns(false);
        _currentUserServiceMock
            .Setup(s => s.IsSuperAdmin)
            .Returns(true);

        // Act
        var query = _roleService.QueryRoles();
        var result = query.ToArray();

        // Assert

        using (new AssertionScope())
        {
            result.Should().HaveCount(3);

            foreach (var dto in result)
            {
                var item = items.Single(u => u.Id == dto.Id);

                dto.Should().BeEquivalentTo(item, opt => opt.ExcludingMissingMembers());

                dto.AssignedUsersCount.Should().Be(2);
            }
        }
    }
}
