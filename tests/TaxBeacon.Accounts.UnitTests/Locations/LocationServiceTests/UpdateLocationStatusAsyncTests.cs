using FluentAssertions.Execution;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TaxBeacon.Common.Enums.Administration.Activities;
using TaxBeacon.Common.Enums;
using Moq;

namespace TaxBeacon.Accounts.UnitTests.Locations;
public partial class LocationServiceTests
{
    [Theory]
    [InlineData(Status.Active)]
    [InlineData(Status.Deactivated)]
    public async Task UpdateLocationStatusAsync_LocationExists_UpdatedLocation(Status status)
    {
        //Arrange
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker
            .RuleFor(x => x.Tenant, tenant)
            .Generate();
        var location = TestData.LocationFaker
            .RuleFor(x => x.TenantId, tenant.Id)
            .RuleFor(x => x.AccountId, account.Id)
            .Generate();

        var currentDate = DateTime.UtcNow;
        _currentUserServiceMock
            .Setup(service => service.TenantId)
            .Returns(tenant.Id);
        _dateTimeServiceMock
            .Setup(service => service.UtcNow)
            .Returns(currentDate);

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Locations.AddAsync(location);
        await _dbContextMock.SaveChangesAsync();

        //Act
        var actualResult = await _locationService.UpdateLocationStatusAsync(account.Id, location.Id, status);

        //Assert
        using (new AssertionScope())
        {
            actualResult.TryPickT0(out var updatedLocation, out _).Should().BeTrue();
            (await _dbContextMock.SaveChangesAsync()).Should().Be(0);
            updatedLocation.Status.Should().Be(status);

            var actualActivityLog = await _dbContextMock.LocationActivityLogs.LastOrDefaultAsync();
            actualActivityLog.Should().NotBeNull();
            actualActivityLog?.Date.Should().Be(currentDate);
            actualActivityLog?.EventType.Should().Be(status == Status.Active ? LocationEventType.LocationReactivated : LocationEventType.LocationDeactivated);
            actualActivityLog?.TenantId.Should().Be(tenant.Id);
            actualActivityLog?.LocationId.Should().Be(location.Id);

            _dateTimeServiceMock
                .Verify(ds => ds.UtcNow, Times.Exactly(1));
        }
    }

    [Fact]
    public async Task UpdateLocationStatusAsync_LocationDoesNotExist_NotFound()
    {
        //Arrange
        var tenant = TestData.TenantFaker.Generate();
        var location = TestData.LocationFaker
            .RuleFor(x => x.TenantId, tenant.Id)
            .Generate();

        _currentUserServiceMock
            .Setup(service => service.TenantId)
            .Returns(tenant.Id);

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Locations.AddAsync(location);
        await _dbContextMock.SaveChangesAsync();

        //Act
        var actualResult = await _locationService.UpdateLocationStatusAsync(new Guid(), new Guid(), Status.Active);

        //Assert
        actualResult.IsT1.Should().BeTrue();
    }
}
