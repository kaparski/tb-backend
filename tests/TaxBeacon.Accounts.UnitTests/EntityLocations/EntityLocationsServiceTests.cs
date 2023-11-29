using Bogus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using TaxBeacon.Accounts.EntityLocations;
using TaxBeacon.Accounts.UnitTests.Locations;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Enums.Accounts;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL;
using TaxBeacon.DAL.Accounts.Entities;
using TaxBeacon.DAL.Administration.Entities;
using TaxBeacon.DAL.Interceptors;

namespace TaxBeacon.Accounts.UnitTests.EntityLocations;

public partial class EntityLocationsServiceTests
{
    private readonly TaxBeaconDbContext _dbContextMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly EntityLocationsService _entityLocationsService;
    private readonly Mock<IDateTimeService> _dateTimeServiceMock;
    private readonly Mock<ILogger<EntityLocationsService>> _loggerMock;

    public EntityLocationsServiceTests()
    {
        _currentUserServiceMock = new();
        _dateTimeServiceMock = new();
        _loggerMock = new();
        Mock<EntitySaveChangesInterceptor> entitySaveChangesInterceptorMock = new();
        _dbContextMock = new TaxBeaconDbContext(
            new DbContextOptionsBuilder<TaxBeaconDbContext>()
                .UseInMemoryDatabase($"{nameof(LocationServiceTests)}-InMemoryDb-{Guid.NewGuid()}")
                .Options,
            entitySaveChangesInterceptorMock.Object);

        _entityLocationsService = new EntityLocationsService(
            _dbContextMock,
            _currentUserServiceMock.Object,
            _dateTimeServiceMock.Object,
            _loggerMock.Object);
    }

    private static class TestData
    {
        public static Faker<Location> LocationFaker =>
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
                .RuleFor(e => e.PrimaryNaicsCode, f => f.Random.Int(100000, 999999))
                .RuleFor(t => t.Status, t => Status.Active);

        public static Faker<Tenant> TenantFaker =>
            new Faker<Tenant>()
                .RuleFor(t => t.Id, f => Guid.NewGuid())
                .RuleFor(t => t.Name, f => f.Company.CompanyName())
                .RuleFor(t => t.CreatedDateTimeUtc, f => DateTime.UtcNow);

        public static Faker<Account> AccountFaker =>
            new Faker<Account>()
                .RuleFor(a => a.Id, f => Guid.NewGuid())
                .RuleFor(a => a.Website, f => f.Internet.Url())
                .RuleFor(a => a.Name, f => f.Company.CompanyName())
                .RuleFor(a => a.AccountId, f => f.Random.String(10))
                .RuleFor(a => a.Country, f => f.Address.Country());

        public static readonly Faker<Entity> EntityFaker =
            new Faker<Entity>()
                .RuleFor(t => t.Id, f => Guid.NewGuid())
                .RuleFor(t => t.EntityId, f => Guid.NewGuid().ToString())
                .RuleFor(t => t.Name, f => f.Company.CompanyName())
                .RuleFor(t => t.Type, t => AccountEntityType.LLC.Name)
                .RuleFor(t => t.City, f => f.Address.City())
                .RuleFor(t => t.State, t => State.NM)
                .RuleFor(t => t.CreatedDateTimeUtc, f => DateTime.UtcNow)
                .RuleFor(t => t.Status, t => Status.Active)
                .RuleFor(t => t.Country, f => f.PickRandom<Country>(Country.List).ToString());

        public static readonly Faker<EntityLocation> EntityLocationFaker =
            new Faker<EntityLocation>()
                .RuleFor(t => t.EntityId, _ => Guid.NewGuid())
                .RuleFor(t => t.LocationId, _ => Guid.NewGuid());
    }
}
