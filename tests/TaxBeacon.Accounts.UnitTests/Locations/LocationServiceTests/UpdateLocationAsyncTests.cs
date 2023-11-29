using FluentAssertions.Execution;
using FluentAssertions;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Moq;
using TaxBeacon.Accounts.Locations.Models;
using TaxBeacon.Common.Enums.Administration.Activities;

namespace TaxBeacon.Accounts.UnitTests.Locations;
public partial class LocationServiceTests
{
    [Fact]
    public async Task UpdateLocationAsync_SuccessfulUpdate_ReturnsLocationDetailsDto()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();
        var location = TestData.LocationFaker
            .RuleFor(e => e.TenantId, _ => tenant.Id)
            .RuleFor(e => e.AccountId, _ => account.Id)
            .Generate();

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.Locations.AddAsync(location);
        await _dbContextMock.SaveChangesAsync();

        var updateLocationDto = TestData.UpdateLocationDtoFaker.Generate();
        var expectedLocation = updateLocationDto.Adapt(location);

        var currentDate = DateTime.UtcNow;
        _dateTimeServiceMock
            .Setup(service => service.UtcNow)
            .Returns(currentDate);

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);

        // Act
        var actualResult = await _locationService.UpdateLocationAsync(account.Id, location.Id, updateLocationDto, default);

        // Assert
        using (new AssertionScope())
        {
            actualResult.TryPickT0(out var updatedLocation, out _).Should().BeTrue();
            (await _dbContextMock.SaveChangesAsync()).Should().Be(0);
            updatedLocation.Should().BeEquivalentTo(expectedLocation, opt =>
            {
                opt.Excluding(x => x.Phones);
                opt.Excluding(x => x.Id);
                opt.ExcludingMissingMembers();
                return opt;
            });
            updatedLocation.Phones.Should().BeEquivalentTo(updateLocationDto.Phones, opt => opt.ExcludingMissingMembers());
            var actualActivityLog = await _dbContextMock.LocationActivityLogs.LastOrDefaultAsync();
            actualActivityLog.Should().NotBeNull();
            actualActivityLog?.Date.Should().Be(currentDate);
            actualActivityLog?.EventType.Should().Be(LocationEventType.LocationUpdated);
            actualActivityLog?.TenantId.Should().Be(tenant.Id);
            actualActivityLog?.LocationId.Should().Be(location.Id);

            _dateTimeServiceMock
                .Verify(ds => ds.UtcNow, Times.Once);
        }
    }

    [Fact]
    public async Task UpdateLocationAsync_LocationAccountNotExistInDb_ReturnsNotFound()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        await _dbContextMock.Tenants.AddAsync(tenant);
        var location = TestData.LocationFaker
            .RuleFor(l => l.TenantId, _ => tenant.Id)
            .RuleFor(l => l.AccountId, _ => Guid.NewGuid())
            .Generate();

        await _dbContextMock.Locations.AddAsync(location);

        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);

        // Act
        var actualResult = await _locationService.UpdateLocationAsync(Guid.NewGuid(), location.Id, new UpdateLocationDto());

        // Assert
        using (new AssertionScope())
        {
            actualResult.IsT0.Should().BeFalse();
            actualResult.IsT1.Should().BeTrue();
        }
    }

    [Fact]
    public async Task UpdateLocationAsync_LocationNotExistsInDb_ReturnsNotFound()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);

        // Act
        var actualResult = await _locationService.UpdateLocationAsync(Guid.NewGuid(), Guid.NewGuid(), new UpdateLocationDto());

        // Assert
        using (new AssertionScope())
        {
            actualResult.IsT0.Should().BeFalse();
            actualResult.IsT1.Should().BeTrue();
        }
    }
}
