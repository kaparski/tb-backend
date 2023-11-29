using Bogus;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;
using TaxBeacon.Accounts.Common.Models;
using TaxBeacon.Accounts.Locations;
using TaxBeacon.Accounts.Locations.Activities.Factories;
using TaxBeacon.Accounts.Locations.Activities.Models;
using TaxBeacon.Accounts.Locations.Models;
using TaxBeacon.Common.Converters;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Enums.Accounts;
using TaxBeacon.Common.Enums.Administration.Activities;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL;
using TaxBeacon.DAL.Accounts.Entities;
using TaxBeacon.DAL.Administration.Entities;
using TaxBeacon.DAL.Interceptors;

namespace TaxBeacon.Accounts.UnitTests.Locations;

public partial class LocationServiceTests
{
    private readonly TaxBeaconDbContext _dbContextMock;
    private readonly LocationService _locationService;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IDateTimeService> _dateTimeServiceMock;
    private readonly Mock<ILogger<LocationService>> _locationServiceLoggerMock;
    private readonly Mock<IEnumerable<ILocationActivityFactory>> _activityFactoriesMock = new();
    private readonly Mock<IListToFileConverter> _csvMock = new();
    private readonly Mock<IListToFileConverter> _xlsxMock = new();
    private readonly Mock<IEnumerable<IListToFileConverter>> _listToFileConverters = new();
    private readonly Mock<IDateTimeFormatter> _dateTimeFormatterMock = new();

    public LocationServiceTests()
    {
        _currentUserServiceMock = new();
        _dateTimeServiceMock = new();
        _locationServiceLoggerMock = new();
        Mock<EntitySaveChangesInterceptor> entitySaveChangesInterceptorMock = new();
        _dbContextMock = new TaxBeaconDbContext(
            new DbContextOptionsBuilder<TaxBeaconDbContext>()
                .UseInMemoryDatabase($"{nameof(LocationServiceTests)}-InMemoryDb-{Guid.NewGuid()}")
                .Options,
            entitySaveChangesInterceptorMock.Object);
        _activityFactoriesMock
            .Setup(x => x.GetEnumerator())
            .Returns(new ILocationActivityFactory[]
            {
                new LocationUpdatedEventFactory(),
                new LocationCreatedEventFactory()
            }.ToList().GetEnumerator());

        _csvMock.Setup(x => x.FileType).Returns(FileType.Csv);
        _xlsxMock.Setup(x => x.FileType).Returns(FileType.Xlsx);
        _listToFileConverters
            .Setup(x => x.GetEnumerator())
            .Returns(new[] { _csvMock.Object, _xlsxMock.Object }.ToList()
                .GetEnumerator());

        _locationService = new LocationService(
            _dbContextMock,
            _currentUserServiceMock.Object,
            _dateTimeServiceMock.Object,
            _locationServiceLoggerMock.Object,
            _activityFactoriesMock.Object,
            _listToFileConverters.Object,
            _dateTimeFormatterMock.Object);

        TypeAdapterConfig.GlobalSettings.Scan(typeof(ILocationService).Assembly);
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
                .RuleFor(a => a.AccountId, f => f.Random.String(10))
                .RuleFor(a => a.Name, f => f.Company.CompanyName())
                .RuleFor(a => a.Country, f => f.Address.Country());

        public static Faker<LocationPhone> PhoneFaker =>
            new Faker<LocationPhone>()
                .RuleFor(p => p.Id, _ => Guid.NewGuid())
                .RuleFor(p => p.Number, f => f.Phone.PhoneNumber())
                .RuleFor(p => p.Type, f => f.PickRandom<PhoneType>());

        private static Faker<CreateUpdatePhoneDto> CreateUpdatePhoneDtoFaker =>
            new Faker<CreateUpdatePhoneDto>()
                .RuleFor(p => p.Number, f => f.Phone.PhoneNumber())
                .RuleFor(p => p.Type, f => f.PickRandom<PhoneType>())
                .RuleFor(p => p.Extension, f => f.PickRandom(1, 20).ToString());

        public static Faker<CreateLocationDto> CreateLocationFaker =>
            new Faker<CreateLocationDto>()
                .RuleFor(e => e.Name, f => f.Company.CompanyName())
                .RuleFor(e => e.LocationId, f => f.Company.CompanySuffix())
                .RuleFor(e => e.Country, f => f.PickRandom<Country>(Country.List))
                .RuleFor(e => e.Address1,
                    (f, e) => e.Country == Country.UnitedStates ? f.Address.StreetAddress() : null)
                .RuleFor(e => e.Address2,
                    (f, e) => e.Country == Country.UnitedStates ? f.Address.SecondaryAddress() : null)
                .RuleFor(e => e.City,
                    (f, e) => e.Country == Country.UnitedStates ? f.Address.City() : null)
                .RuleFor(e => e.State,
                    (f, e) => e.Country == Country.UnitedStates ? f.PickRandom<State>() : null)
                .RuleFor(e => e.County,
                    (f, e) => e.Country == Country.UnitedStates ? f.Address.County() : null)
                .RuleFor(e => e.Zip,
                    (f, e) => e.Country == Country.UnitedStates ? f.Random.Number(10000, 999999999).ToString() : null)
                .RuleFor(e => e.Address,
                    (f, e) => e.Country == Country.International ? f.Address.FullAddress() : null)
                .RuleFor(e => e.Type, f => f.PickRandom<LocationType>())
                .RuleFor(e => e.PrimaryNaicsCode, f => f.Random.Int(100000, 999999))
                .RuleFor(e => e.Phones, _ => CreateUpdatePhoneDtoFaker.Generate(2));

        public static Faker<UpdateLocationDto> UpdateLocationDtoFaker =>
            new Faker<UpdateLocationDto>()
                .RuleFor(u => u.Name, f => f.Company.CompanyName())
                .RuleFor(u => u.Country, f => f.PickRandom<Country>(Country.List))
                .RuleFor(u => u.State, f => f.PickRandom<State>())
                .RuleFor(u => u.City, f => f.Address.City())
                .RuleFor(u => u.Address1, f => f.Address.StreetAddress())
                .RuleFor(u => u.Address2, f => f.Address.StreetAddress())
                .RuleFor(u => u.Zip, f => f.Random.Number(10000, 9999999).ToString())
                .RuleFor(u => u.County, f => f.Address.County());

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

        public static readonly Faker<LocationActivityLog> LocationActivityFaker =
            new Faker<LocationActivityLog>()
                .RuleFor(l => l.Date, f => f.Date.Recent())
                .RuleFor(l => l.Revision, _ => 1u)
                .RuleFor(l => l.Event, (f, x) => JsonSerializer.Serialize(
                    new LocationCreatedEvent(Guid.NewGuid(), f.Name.JobTitle(), f.Name.FullName(), DateTime.Now)
                ))
                .RuleFor(l => l.EventType, f => LocationEventType.LocationCreated);
    }
}
