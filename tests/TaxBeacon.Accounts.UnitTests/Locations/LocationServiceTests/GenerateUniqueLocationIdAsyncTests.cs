using FluentAssertions;

namespace TaxBeacon.Accounts.UnitTests.Locations;
public partial class LocationServiceTests
{
    [Fact]
    public async Task GenerateUniqueLocationIdAsync_LocationDoesNotExistInDb_ReturnsNotFound()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();
        var idIncrement = 1_000_000;
        var location = TestData.LocationFaker
            .RuleFor(e => e.TenantId, _ => tenant.Id)
            .RuleFor(e => e.AccountId, _ => account.Id)
            .RuleFor(e => e.LocationId, () => "L" + idIncrement++.ToString())
            .Generate(10000);

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.Locations.AddRangeAsync(location);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);
        var locationIds = location.Select(x => x.LocationId).ToHashSet();
        for (var i = 0; i < 100; i++)
        {
            var actualResult = await _locationService.GenerateUniqueLocationIdAsync(CancellationToken.None);

            actualResult.IsT0.Should().BeTrue();
            locationIds.Should().NotContain(actualResult.AsT0);
        }
    }
}
