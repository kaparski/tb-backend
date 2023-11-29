using FluentAssertions.Execution;
using FluentAssertions;

namespace TaxBeacon.Accounts.UnitTests.Locations;
public partial class LocationServiceTests
{
    [Fact]
    public async Task GetActivitiesAsync_LocationExistsInDb_ReturnsActivities()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();
        var location = TestData.LocationFaker
            .RuleFor(l => l.TenantId, _ => tenant.Id)
            .RuleFor(l => l.AccountId, _ => account.Id)
            .Generate();
        var locationActivities = TestData.LocationActivityFaker
            .RuleFor(l => l.LocationId, _ => location.Id)
            .RuleFor(l => l.TenantId, _ => tenant.Id)
            .Generate(2);
        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.Locations.AddAsync(location);
        await _dbContextMock.LocationActivityLogs.AddRangeAsync(locationActivities);

        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);

        // Act
        var actualResult = await _locationService.GetActivitiesAsync(account.Id, location.Id);

        // Assert
        using (new AssertionScope())
        {
            actualResult.TryPickT0(out var activity, out _).Should().BeTrue();
            activity.Query.Count().Should().Be(locationActivities.Count);
            actualResult.IsT1.Should().BeFalse();
        }
    }

    [Fact]
    public async Task GetActivitiesAsync_LocationDoesNotExistInDb_ReturnsNotFound()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        await _dbContextMock.Tenants.AddAsync(tenant);

        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);

        // Act
        var actualResult = await _locationService.GetActivitiesAsync(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        using (new AssertionScope())
        {
            actualResult.IsT0.Should().BeFalse();
            actualResult.IsT1.Should().BeTrue();
        }
    }
}
