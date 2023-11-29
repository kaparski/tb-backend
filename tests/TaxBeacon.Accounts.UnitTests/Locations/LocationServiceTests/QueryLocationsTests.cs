using FluentAssertions.Execution;
using FluentAssertions;
using Mapster;
using OneOf.Types;
using TaxBeacon.Accounts.Locations.Models;

namespace TaxBeacon.Accounts.UnitTests.Locations;
public partial class LocationServiceTests
{
    [Fact]
    public async Task QueryLocations_AccountExists_ReturnsLocations()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker
            .RuleFor(acc => acc.Tenant, f => tenant)
            .Generate();
        var locations = TestData.LocationFaker
            .RuleFor(l => l.Account, f => account)
            .Generate(5);

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddRangeAsync(account);
        await _dbContextMock.Locations.AddRangeAsync(locations);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock.Setup(x => x.TenantId).Returns(tenant.Id);

        // Act
        var oneOf = _locationService.QueryLocations(account.Id);

        // Assert
        using (new AssertionScope())
        {
            oneOf.TryPickT0(out var locationDtos, out _).Should().BeTrue();
            locationDtos.Should().HaveCount(5);

            foreach (var dto in locationDtos)
            {
                var location = locations.Single(u => u.Id == dto.Id);

                dto.Should().BeEquivalentTo(location.Adapt<LocationDto>());
            }
        }
    }

    [Fact]
    public async Task QueryLocations_AccountDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker
            .RuleFor(acc => acc.Tenant, f => tenant)
            .Generate();
        var locations = TestData.LocationFaker
            .RuleFor(l => l.Account, f => account)
            .Generate(5);

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddRangeAsync(account);
        await _dbContextMock.Locations.AddRangeAsync(locations);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock.Setup(x => x.TenantId).Returns(tenant.Id);

        // Act
        var oneOf = _locationService.QueryLocations(Guid.NewGuid());

        // Assert
        using (new AssertionScope())
        {
            oneOf.TryPickT1(out var notFound, out _).Should().BeTrue();
            notFound.Should().BeOfType<NotFound>();
        }
    }
}
