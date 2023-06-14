using Bogus;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.EntityFrameworkCore;
using Moq;
using OneOf.Types;
using System.Diagnostics.CodeAnalysis;
using TaxBeacon.Accounts.Locations;
using TaxBeacon.Accounts.Services.Contacts;
using TaxBeacon.Common.Accounts;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL;
using TaxBeacon.DAL.Entities;
using TaxBeacon.DAL.Entities.Accounts;
using TaxBeacon.DAL.Interceptors;

namespace TaxBeacon.Accounts.UnitTests.Locations;

public class LocationServiceTests
{
    private readonly TaxBeaconDbContext _dbContext;
    private readonly LocationService _locationService;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;

    public LocationServiceTests()
    {
        _currentUserServiceMock = new();
        Mock<EntitySaveChangesInterceptor> entitySaveChangesInterceptorMock = new();
        _dbContext = new TaxBeaconDbContext(
            new DbContextOptionsBuilder<TaxBeaconDbContext>()
                .UseInMemoryDatabase($"{nameof(LocationServiceTests)}-InMemoryDb-{Guid.NewGuid()}")
                .Options,
            entitySaveChangesInterceptorMock.Object);

        _locationService = new LocationService(_dbContext, _currentUserServiceMock.Object);

    }
  
    [Fact]
    public async Task QueryLocations_AccountExists_ReturnsLocations()
    {
        // Arrange
        var tenant = TestData.TestTenant.Generate();
        var account = TestData.TestAccount
            .RuleFor(acc => acc.Tenant, f => tenant)
            .Generate();
        var locations = TestData.TestLocation
            .RuleFor(l => l.Account, f => account)
            .Generate(5);
        
        await _dbContext.Tenants.AddAsync(tenant);
        await _dbContext.Accounts.AddRangeAsync(account);
        await _dbContext.Locations.AddRangeAsync(locations);
        await _dbContext.SaveChangesAsync();
        
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

                dto.Should().BeEquivalentTo(location, 
                    opt => opt.ExcludingMissingMembers());
            }
        }
    }

    [Fact]
    public async Task QueryLocations_AccountDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var tenant = TestData.TestTenant.Generate();
        var account = TestData.TestAccount
            .RuleFor(acc => acc.Tenant, f => tenant)
            .Generate();
        var locations = TestData.TestLocation
            .RuleFor(l => l.Account, f => account)
            .Generate(5);
        
        await _dbContext.Tenants.AddAsync(tenant);
        await _dbContext.Accounts.AddRangeAsync(account);
        await _dbContext.Locations.AddRangeAsync(locations);
        await _dbContext.SaveChangesAsync();
        
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

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    private static class TestData
    {
        public static readonly Faker<Location> TestLocation =
            new Faker<Location>()
                .RuleFor(t => t.Id, f => Guid.NewGuid())
                .RuleFor(l => l.Name, f => f.Lorem.Word())
                .RuleFor(l => l.LocationId, f => Guid.NewGuid().ToString())
                .RuleFor(l => l.Country, f => "United States")
                .RuleFor(l => l.State, f => f.PickRandom<State>())
                .RuleFor(l => l.County, f => f.Address.County())
                .RuleFor(l => l.City, f => f.Address.City())
                .RuleFor(t => t.Type, t => t.PickRandom<LocationType>())
                .RuleFor(t => t.CreatedDateTimeUtc, f => DateTime.UtcNow)
                .RuleFor(t => t.Status, t => Status.Active);

        public static readonly Faker<Tenant> TestTenant =
            new Faker<Tenant>()
                .RuleFor(t => t.Id, f => Guid.NewGuid())
                .RuleFor(t => t.Name, f => f.Company.CompanyName())
                .RuleFor(t => t.CreatedDateTimeUtc, f => DateTime.UtcNow);

        public static readonly Faker<Account> TestAccount =
            new Faker<Account>()
                .RuleFor(t => t.Id, f => Guid.NewGuid())
                .RuleFor(t => t.Website, f => f.Internet.Url())
                .RuleFor(t => t.Name, f => f.Company.CompanyName());
    }
}
