using Bogus;
using FluentAssertions;
using FluentAssertions.Execution;
using System.Net.Mail;
using TaxBeacon.DAL.Administration.Entities;

namespace TaxBeacon.Administration.UnitTests.Services.Users;

public partial class UserServiceTests
{
    [Fact]
    public async Task GetUserInfoAsync_UserExists_ReturnsUserInfo()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        var user = TestData.UserFaker.Generate();
        var tenantRole = new Role { Id = Guid.NewGuid(), Name = new Faker().Random.Word() };
        var role = new Role { Id = Guid.NewGuid(), Name = new Faker().Random.Word() };

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Roles.AddRangeAsync(tenantRole, role);
        await _dbContextMock.TenantUsers.AddAsync(new TenantUser { Tenant = tenant, User = user });
        await _dbContextMock.TenantRoles.AddAsync(new TenantRole { Tenant = tenant, Role = tenantRole });
        await _dbContextMock.TenantUserRoles.AddAsync(new TenantUserRole
        {
            TenantId = tenant.Id,
            UserId = user.Id,
            RoleId = tenantRole.Id
        });
        await _dbContextMock.UserRoles.AddAsync(new UserRole { UserId = user.Id, RoleId = role.Id });
        await _dbContextMock.SaveChangesAsync();

        // Act
        var actualResult = await _userService.GetUserInfoAsync(new MailAddress(user.Email), default);

        _currentUserServiceMock
            .Setup(service => service.TenantId)
            .Returns(tenant.Id);

        _currentUserServiceMock
            .Setup(service => service.IsSuperAdmin)
            .Returns(false);

        // Assert
        using (new AssertionScope())
        {
            actualResult.Should().NotBeNull();
            actualResult!.FullName.Should().Be(user.FullName);
            actualResult.Roles.Should().BeEmpty();
            actualResult.TenantRoles.Should().BeEquivalentTo(tenantRole.Name);
            actualResult.TenantId.Should().Be(tenant.Id);
        }
    }

    [Fact]
    public async Task GetUserInfoAsync_UserDoesNotExistsReturnsNull()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(service => service.TenantId)
            .Returns(tenant.Id);

        _currentUserServiceMock
            .Setup(service => service.IsSuperAdmin)
            .Returns(false);

        // Act
        var actualResult = await _userService.GetUserInfoAsync(new MailAddress("notexistes@mail.com"), default);

        // Assert
        actualResult.Should().BeNull();
    }
}
