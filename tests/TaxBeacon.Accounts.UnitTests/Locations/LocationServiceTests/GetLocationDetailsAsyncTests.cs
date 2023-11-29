using FluentAssertions.Execution;
using FluentAssertions;

namespace TaxBeacon.Accounts.UnitTests.Locations;
public partial class LocationServiceTests
{
    [Fact]
    public async Task GetLocationDetailsAsync_LocationExists_ReturnsLocation()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker.Generate();
        var location = TestData.LocationFaker
            .RuleFor(l => l.TenantId, _ => tenant.Id)
            .RuleFor(l => l.AccountId, _ => account.Id)
            .RuleFor(l => l.Phones, _ => TestData.PhoneFaker.Generate(3))
            .Generate();

        await _dbContextMock.Locations.AddAsync(location);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(x => x.TenantId)
            .Returns(tenant.Id);

        // Act
        var actualResult = await _locationService.GetLocationDetailsAsync(account.Id, location.Id, default);

        // Assert
        using (new AssertionScope())
        {
            actualResult.TryPickT0(out var actualLocation, out _).Should().BeTrue();
            actualLocation.Should().BeEquivalentTo(location, opt => opt.ExcludingMissingMembers());
            actualLocation.Phones.Should().BeEquivalentTo(location.Phones, opt => opt.ExcludingMissingMembers());
        }
    }

    [Fact]
    public async Task GetLocationDetailsAsync_LocationDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker.Generate();
        var location = TestData.LocationFaker
            .RuleFor(l => l.TenantId, _ => tenant.Id)
            .RuleFor(l => l.AccountId, _ => account.Id)
            .RuleFor(l => l.Phones, _ => TestData.PhoneFaker.Generate(3))
            .Generate();

        await _dbContextMock.Locations.AddAsync(location);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(x => x.TenantId)
            .Returns(tenant.Id);

        // Act
        var actualResult = await _locationService.GetLocationDetailsAsync(account.Id, new Guid(), default);

        // Assert
        using (new AssertionScope())
        {
            actualResult.IsT1.Should().BeTrue();
            actualResult.IsT0.Should().BeFalse();
        }
    }

    [Fact]
    public async Task GetLocationDetailsAsync_LocationTenantIdNotEqualCurrentUserTenantId_ReturnsNotFound()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker.Generate();
        var location = TestData.LocationFaker
            .RuleFor(l => l.TenantId, _ => tenant.Id)
            .RuleFor(l => l.AccountId, _ => account.Id)
            .RuleFor(l => l.Phones, _ => TestData.PhoneFaker.Generate(3))
            .Generate();

        await _dbContextMock.Locations.AddAsync(location);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(x => x.TenantId)
            .Returns(new Guid());

        // Act
        var actualResult = await _locationService.GetLocationDetailsAsync(account.Id, new Guid(), default);

        // Assert
        using (new AssertionScope())
        {
            actualResult.IsT1.Should().BeTrue();
            actualResult.IsT0.Should().BeFalse();
        }
    }
}
