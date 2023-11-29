using FluentAssertions;
using FluentAssertions.Execution;
using Moq;
using TaxBeacon.Common.Enums;
using TaxBeacon.DAL.Administration.Entities;

namespace TaxBeacon.Administration.UnitTests.Services.Users;

public partial class UserServiceTests
{
    [Fact]
    public async Task UpdateUserStatusAsync_ActiveUserStatusAndUserId_UpdatedUser()
    {
        //Arrange
        var tenant = TestData.TenantFaker.Generate();
        var user = TestData.UserFaker.Generate();
        var currentDate = DateTime.UtcNow;

        user.TenantUsers.Add(new TenantUser { Tenant = tenant });
        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Users.AddAsync(user);
        await _dbContextMock.SaveChangesAsync();

        _dateTimeServiceMock
            .Setup(ds => ds.UtcNow)
            .Returns(currentDate);

        _currentUserServiceMock
            .Setup(s => s.TenantRoles)
            .Returns(Array.Empty<string>());

        _currentUserServiceMock
            .Setup(s => s.Roles)
            .Returns(Array.Empty<string>());

        _currentUserServiceMock
            .Setup(service => service.TenantId)
            .Returns(tenant.Id);

        _currentUserServiceMock
            .Setup(service => service.IsSuperAdmin)
            .Returns(false);

        //Act
        var actualResult = await _userService.UpdateUserStatusAsync(user.Id, Status.Active);

        //Assert
        using (new AssertionScope())
        {
            actualResult.TryPickT0(out var updatedUser, out _).Should().BeTrue();
            (await _dbContextMock.SaveChangesAsync()).Should().Be(0);
            updatedUser.Status.Should().Be(Status.Active);
            updatedUser.DeactivationDateTimeUtc.Should().BeNull();
            updatedUser.ReactivationDateTimeUtc.Should().Be(currentDate);

            _dateTimeServiceMock
                .Verify(ds => ds.UtcNow, Times.Exactly(3));
        }
    }

    [Fact]
    public async Task UpdateUserStatusAsync_DeactivatedUserStatusAndUserId_UpdatedUser()
    {
        //Arrange
        var tenant = TestData.TenantFaker.Generate();
        var user = TestData.UserFaker.Generate();
        var currentDate = DateTime.UtcNow;

        user.TenantUsers.Add(new TenantUser { Tenant = tenant });
        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Users.AddAsync(user);
        await _dbContextMock.SaveChangesAsync();

        _dateTimeServiceMock
            .Setup(ds => ds.UtcNow)
            .Returns(currentDate);

        _currentUserServiceMock
            .Setup(s => s.TenantRoles)
            .Returns(Array.Empty<string>());

        _currentUserServiceMock
            .Setup(s => s.Roles)
            .Returns(Array.Empty<string>());

        _currentUserServiceMock
            .Setup(service => service.TenantId)
            .Returns(tenant.Id);

        _currentUserServiceMock
            .Setup(service => service.IsSuperAdmin)
            .Returns(false);

        //Act
        var actualResult = await _userService.UpdateUserStatusAsync(user.Id, Status.Deactivated);

        //Assert
        using (new AssertionScope())
        {
            actualResult.TryPickT0(out var updatedUser, out _).Should().BeTrue();
            (await _dbContextMock.SaveChangesAsync()).Should().Be(0);
            updatedUser.Status.Should().Be(Status.Deactivated);
            updatedUser.ReactivationDateTimeUtc.Should().BeNull();
            updatedUser.DeactivationDateTimeUtc.Should().Be(currentDate);

            _dateTimeServiceMock
                .Verify(ds => ds.UtcNow, Times.Exactly(4));
            // TODO: Verify logs
        }
    }
}
