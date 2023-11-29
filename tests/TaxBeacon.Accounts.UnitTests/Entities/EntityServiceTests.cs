using Bogus;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using TaxBeacon.Accounts.Common.Models;
using TaxBeacon.Accounts.Common.Services;
using TaxBeacon.Accounts.Entities;
using TaxBeacon.Accounts.Entities.Activities.Factories;
using TaxBeacon.Accounts.Entities.Exporters;
using TaxBeacon.Accounts.Entities.Models;
using TaxBeacon.Accounts.Naics;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Enums.Accounts;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL;
using TaxBeacon.DAL.Accounts.Entities;
using TaxBeacon.DAL.Administration.Entities;
using TaxBeacon.DAL.Interceptors;

namespace TaxBeacon.Accounts.UnitTests.Entities;

public partial class EntityServiceTests
{
    private readonly Mock<IDateTimeService> _dateTimeServiceMock = new();
    private readonly Mock<ILogger<EntityService>> _entityServiceLoggerMock = new();
    private readonly TaxBeaconDbContext _dbContextMock;
    private readonly IEntityService _entityService;
    private readonly Mock<EntitySaveChangesInterceptor> _entitySaveChangesInterceptorMock = new();
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();
    private readonly Mock<IEnumerable<IEntityActivityFactory>> _activityFactoriesMock = new();
    private readonly Mock<IDateTimeFormatter> _dateTimeFormatterMock = new();
    private readonly Mock<INaicsService> _naicsServiceMock = new();
    private readonly Mock<IAccountEntitiesToCsvExporter> _accountEntitiesToCsvExporter = new();
    private readonly Mock<IAccountEntitiesToXlsxExporter> _accountEntitiesToXlsxExporter = new();
    private readonly Mock<ICsvService> _csvServiceMock = new();

    public EntityServiceTests()
    {
        _dbContextMock = new TaxBeaconDbContext(
            new DbContextOptionsBuilder<TaxBeaconDbContext>()
                .UseInMemoryDatabase($"{nameof(EntityServiceTests)}-InMemoryDb-{Guid.NewGuid()}")
                .Options,
            _entitySaveChangesInterceptorMock.Object);

        _activityFactoriesMock
            .Setup(x => x.GetEnumerator())
            .Returns(new IEntityActivityFactory[] { new EntityUpdatedEventFactory() }.ToList().GetEnumerator());

        _entityService = new EntityService(
            _entityServiceLoggerMock.Object,
            _dateTimeServiceMock.Object,
            _dbContextMock,
            _currentUserServiceMock.Object,
            _activityFactoriesMock.Object,
            _dateTimeFormatterMock.Object,
            _accountEntitiesToXlsxExporter.Object,
            _accountEntitiesToCsvExporter.Object,
            _csvServiceMock.Object);

        TypeAdapterConfig.GlobalSettings.Scan(typeof(IEntityService).Assembly);
    }

    private static class TestData
    {
        public static readonly Faker<Entity> EntityFaker =
            new Faker<Entity>()
                .RuleFor(t => t.Id, _ => Guid.NewGuid())
                .RuleFor(t => t.EntityId, f => Guid.NewGuid().ToString())
                .RuleFor(t => t.Name, f => f.Company.CompanyName())
                .RuleFor(t => t.Type, _ => AccountEntityType.LLC.Name)
                .RuleFor(t => t.City, f => f.Address.City())
                .RuleFor(t => t.State, _ => State.NM)
                .RuleFor(t => t.CreatedDateTimeUtc, _ => DateTime.UtcNow)
                .RuleFor(t => t.Status, _ => Status.Active)
                .RuleFor(t => t.Country, f => f.PickRandom<Country>(Country.List).ToString());

        public static readonly Faker<Location> LocationFaker =
            new Faker<Location>()
                .RuleFor(t => t.Id, _ => Guid.NewGuid())
                .RuleFor(t => t.LocationId, f => f.Commerce.Ean13())
                .RuleFor(t => t.Name, f => f.Company.CompanyName())
                .RuleFor(t => t.Type, _ => LocationType.Remote)
                .RuleFor(t => t.City, f => f.Address.City())
                .RuleFor(t => t.State, _ => State.NM)
                .RuleFor(t => t.CreatedDateTimeUtc, _ => DateTime.UtcNow)
                .RuleFor(t => t.Status, _ => Status.Active)
                .RuleFor(t => t.Country, f => f.PickRandom<Country>(Country.List).ToString());

        public static Faker<Tenant> TenantFaker =>
            new Faker<Tenant>()
                .RuleFor(t => t.Id, _ => Guid.NewGuid())
                .RuleFor(t => t.Name, f => f.Company.CompanyName())
                .RuleFor(t => t.CreatedDateTimeUtc, _ => DateTime.UtcNow);

        public static Faker<Account> AccountFaker =>
            new Faker<Account>()
                .RuleFor(a => a.Id, _ => Guid.NewGuid())
                .RuleFor(a => a.Website, f => f.Internet.Url())
                .RuleFor(a => a.AccountId, f => f.Random.String(10))
                .RuleFor(a => a.Name, f => f.Company.CompanyName())
                .RuleFor(a => a.Country, f => f.Address.Country());

        public static Faker<EntityPhone> PhoneFaker =>
            new Faker<EntityPhone>()
                .RuleFor(p => p.Id, _ => Guid.NewGuid())
                .RuleFor(p => p.Number, f => f.Phone.PhoneNumber())
                .RuleFor(p => p.Type, f => f.PickRandom<PhoneType>());

        public static Faker<CreateEntityDto> CreateEntityFaker =>
            new Faker<CreateEntityDto>()
                .RuleFor(e => e.Name, f => f.Company.CompanyName())
                .RuleFor(e => e.EntityId, f => Guid.NewGuid().ToString())
                .RuleFor(e => e.DoingBusinessAs, f => f.Company.CompanySuffix())
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
                .RuleFor(e => e.Fein,
                    (f, e) => e.Country == Country.UnitedStates ? f.Random.Number(10000, 999999999).ToString() : null)
                .RuleFor(e => e.Ein,
                    (f, e) => e.Country == Country.International ? f.Random.Number(10000, 999999999).ToString() : null)
                .RuleFor(e => e.Type, f => f.PickRandom<AccountEntityType>(AccountEntityType.List))
                .RuleFor(e => e.TaxYearEndType, f => f
                    .PickRandom(new List<TaxYearEndType?>(TaxYearEndType.List) { null }))
                .RuleFor(e => e.DateOfIncorporation, _ => DateTime.UtcNow)
                .RuleFor(e => e.PrimaryNaicsCode, f => f.Random.Int(100000, 999999))
                .RuleFor(e => e.Phones, _ => CreateUpdatePhoneDtoFaker.Generate(2));

        public static Faker<UpdateEntityDto> UpdateEntityFaker =>
            new Faker<UpdateEntityDto>()
                .RuleFor(e => e.Name, f => f.Company.CompanyName())
                .RuleFor(e => e.DoingBusinessAs, f => f.Company.CompanySuffix())
                .RuleFor(e => e.Country, _ => Country.UnitedStates)
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
                .RuleFor(e => e.Fein,
                    (f, e) => e.Country == Country.UnitedStates ? f.Random.Number(10000, 999999999).ToString() : null)
                .RuleFor(e => e.Ein,
                    (f, e) => e.Country == Country.International ? f.Random.Number(10000, 999999999).ToString() : null)
                .RuleFor(e => e.Type, f => f.PickRandom<AccountEntityType>(AccountEntityType.List))
                .RuleFor(e => e.TaxYearEndType, f => f.PickRandom<TaxYearEndType>(TaxYearEndType.List))
                .RuleFor(e => e.DateOfIncorporation, _ => DateTime.UtcNow)
                .RuleFor(e => e.PrimaryNaicsCode, f => f.Random.Int(100000, 999999))
                .RuleFor(e => e.Phones, _ => CreateUpdatePhoneDtoFaker.Generate(2));

        public static Faker<StateId> StateIdFaker =>
            new Faker<StateId>()
                .RuleFor(s => s.Id, _ => Guid.NewGuid())
                .RuleFor(p => p.LocalJurisdiction, f => f.Random.String(100))
                .RuleFor(p => p.StateIdCode, f => f.Random.String(25))
                .RuleFor(p => p.State, f => f.PickRandom<State>())
                .RuleFor(p => p.StateIdType, f => f.PickRandom(StateIdType.List.Select(t => t.Name)));

        public static Faker<AddStateIdDto> AddStateIdFaker =>
            new Faker<AddStateIdDto>()
                .RuleFor(p => p.LocalJurisdiction, f => f.Random.String(100))
                .RuleFor(p => p.StateIdCode, f => f.Random.String(25))
                .RuleFor(p => p.State, f => f.PickRandom<State>())
                .RuleFor(p => p.StateIdType, f => f.PickRandom(StateIdType.List.AsEnumerable()));

        private static Faker<CreateUpdatePhoneDto> CreateUpdatePhoneDtoFaker =>
            new Faker<CreateUpdatePhoneDto>()
                .RuleFor(p => p.Number, f => f.Phone.PhoneNumber())
                .RuleFor(p => p.Type, f => f.PickRandom<PhoneType>())
                .RuleFor(p => p.Extension, f => f.PickRandom(1, 20).ToString());

        public static readonly Faker<EntityLocation> EntityLocationFaker =
            new Faker<EntityLocation>()
                .RuleFor(t => t.EntityId, _ => Guid.NewGuid())
                .RuleFor(t => t.LocationId, _ => Guid.NewGuid());

        public static readonly Faker<ImportEntityModel> ImportEntityModelFaker =
            new Faker<ImportEntityModel>()
                .RuleFor(e => e.Name, f => f.Company.CompanyName())
                .RuleFor(e => e.DoingBusinessAs, f => f.Company.CompanySuffix())
                .RuleFor(e => e.Country, _ => Country.UnitedStates.Name)
                .RuleFor(e => e.Address1,
                    (f, e) => e.Country == Country.UnitedStates.Name ? f.Address.StreetAddress() : null)
                .RuleFor(e => e.Address2,
                    (f, e) => e.Country == Country.UnitedStates.Name ? f.Address.SecondaryAddress() : null)
                .RuleFor(e => e.City,
                    (f, e) => e.Country == Country.UnitedStates.Name ? f.Address.City() : null)
                .RuleFor(e => e.State,
                    (f, e) => e.Country == Country.UnitedStates.Name ? f.PickRandom<State>().ToString() : null)
                .RuleFor(e => e.County,
                    (f, e) => e.Country == Country.UnitedStates.Name ? f.Address.County() : null)
                .RuleFor(e => e.Zip,
                    (f, e) => e.Country == Country.UnitedStates.Name
                        ? f.Random.Number(10000, 999999999).ToString()
                        : null)
                .RuleFor(e => e.Address,
                    (f, e) => e.Country == Country.International.Name ? f.Address.FullAddress() : null)
                .RuleFor(e => e.Fein,
                    (f, e) => e.Country == Country.UnitedStates.Name
                        ? f.Random.Number(10000, 999999999).ToString()
                        : null)
                .RuleFor(e => e.Ein,
                    (f, e) => e.Country == Country.International.Name
                        ? f.Random.Number(10000, 999999999).ToString()
                        : null)
                .RuleFor(e => e.Type, f => f.PickRandom<AccountEntityType>(AccountEntityType.List).ToString())
                .RuleFor(e => e.TaxYearEndType, f => f.PickRandom<TaxYearEndType>(TaxYearEndType.List).ToString())
                .RuleFor(e => e.DateOfIncorporation, _ => DateTime.UtcNow)
                .RuleFor(e => e.PrimaryNaicsCode, f => f.Random.Int(100000, 999999));
    }
}
