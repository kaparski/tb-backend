using FluentAssertions;
using FluentAssertions.Execution;
using Moq;
using TaxBeacon.Common.Enums.Administration.Activities;
using TaxBeacon.DAL.Administration.Entities;

namespace TaxBeacon.Administration.UnitTests.Services.Users;

public partial class UserServiceTests
{
    [Fact]
    public async Task GetActivitiesAsync_UserExists_ShouldCallAppropriateFactory()
    {
        //Arrange
        var tenant = TestData.TenantFaker.Generate();
        tenant.Id = _tenantId;
        var user = TestData.UserFaker.Generate();
        var tenantUser = new TenantUser { Tenant = tenant, User = user };
        var userActivity = new UserActivityLog
        {
            Date = DateTime.UtcNow,
            TenantId = tenant.Id,
            UserId = user.Id,
            EventType = UserEventType.UserCreated,
            Revision = 1
        };

        _dbContextMock.Tenants.Add(tenant);
        _dbContextMock.Users.Add(user);
        _dbContextMock.TenantUsers.Add(tenantUser);
        _dbContextMock.UserActivityLogs.Add(userActivity);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(service => service.TenantId)
            .Returns(tenant.Id);

        _currentUserServiceMock
            .Setup(service => service.IsSuperAdmin)
            .Returns(false);

        //Act
        await _userService.GetActivitiesAsync(user.Id);

        //Assert

        _userCreatedActivityFactory.Verify(x => x.Create(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task GetActivitiesAsync_UserDoesNotExistWithinTenant_ShouldReturnNotFount()
    {
        //Arrange
        var tenant = TestData.TenantFaker.Generate();
        tenant.Id = _tenantId;
        var user = TestData.UserFaker.Generate();

        _dbContextMock.Tenants.Add(tenant);
        _dbContextMock.Users.Add(user);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(service => service.TenantId)
            .Returns(tenant.Id);

        _currentUserServiceMock
            .Setup(service => service.IsSuperAdmin)
            .Returns(false);

        //Act
        var resultOneOf = await _userService.GetActivitiesAsync(user.Id);

        //Assert
        resultOneOf.TryPickT1(out _, out _).Should().BeTrue();
    }

    [Fact]
    public async Task GetActivitiesAsync_UserExists_ShouldReturnExpectedNumberOfItems()
    {
        //Arrange
        var tenant = TestData.TenantFaker.Generate();
        tenant.Id = _tenantId;
        var user = TestData.UserFaker.Generate();
        var tenantUser = new TenantUser { Tenant = tenant, User = user };

        var activities = new[]
        {
            new UserActivityLog
            {
                Date = new DateTime(2000, 01, 1),
                TenantId = tenant.Id,
                UserId = user.Id,
                EventType = UserEventType.UserCreated,
                Revision = 1
            },
            new UserActivityLog
            {
                Date = new DateTime(2000, 01, 2),
                TenantId = tenant.Id,
                UserId = user.Id,
                EventType = UserEventType.UserCreated,
                Revision = 1
            },
            new UserActivityLog
            {
                Date = new DateTime(2000, 01, 3),
                TenantId = tenant.Id,
                UserId = user.Id,
                EventType = UserEventType.UserCreated,
                Revision = 1
            }
        };

        _dbContextMock.Tenants.Add(tenant);
        _dbContextMock.Users.Add(user);
        _dbContextMock.TenantUsers.Add(tenantUser);
        _dbContextMock.UserActivityLogs.AddRange(activities);
        await _dbContextMock.SaveChangesAsync();

        const int pageSize = 2;

        _currentUserServiceMock
            .Setup(service => service.TenantId)
            .Returns(tenant.Id);

        _currentUserServiceMock
            .Setup(service => service.IsSuperAdmin)
            .Returns(false);

        //Act
        var resultOneOf = await _userService.GetActivitiesAsync(user.Id, 1, pageSize);

        //Assert
        using (new AssertionScope())
        {
            resultOneOf.TryPickT0(out var activitiesResult, out _).Should().BeTrue();
            activitiesResult.Count.Should().Be(2);
            activitiesResult.Query.Count().Should().Be(2);
        }
    }
}
