using Bogus;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using TaxBeacon.Accounts.Accounts;
using TaxBeacon.Accounts.Accounts.Activities.Factories;
using TaxBeacon.Accounts.Accounts.Models;
using TaxBeacon.Accounts.Common.Models;
using TaxBeacon.Accounts.Naics;
using TaxBeacon.Administration.Users;
using TaxBeacon.Common.Converters;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Enums.Accounts;
using TaxBeacon.Common.Enums.Accounts.Activities;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL;
using TaxBeacon.DAL.Accounts.Entities;
using TaxBeacon.DAL.Administration.Entities;
using TaxBeacon.DAL.Interceptors;
using PhoneType = TaxBeacon.Common.Enums.Accounts.PhoneType;
using Status = TaxBeacon.Common.Enums.Status;
using User = TaxBeacon.DAL.Administration.Entities.User;

namespace TaxBeacon.Accounts.UnitTests.Accounts;

public sealed partial class AccountsServiceTests
{
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<ILogger<AccountService>> _accountServiceLoggerMock;
    private readonly Mock<IEnumerable<IListToFileConverter>> _listToFileConverters;
    private readonly Mock<IListToFileConverter> _csvMock;
    private readonly Mock<IListToFileConverter> _xlsxMock;
    private readonly Mock<IDateTimeService> _dateTimeServiceMock;
    private readonly Mock<EntitySaveChangesInterceptor> _entitySaveChangesInterceptorMock;
    private readonly Mock<IActivityFactory<AccountEventType>> _accountActivityFactoryMock;
    private readonly Mock<IEnumerable<IActivityFactory<AccountEventType>>> _activityFactoriesMock;
    private readonly Mock<IUserService> _userServiceMock;
    private readonly Mock<INaicsService> _naicsServiceMock;
    private readonly TaxBeaconDbContext _dbContextMock;
    private readonly IAccountService _accountService;

    public AccountsServiceTests()
    {
        _currentUserServiceMock = new();
        _entitySaveChangesInterceptorMock = new();
        _accountServiceLoggerMock = new();
        _csvMock = new();
        _xlsxMock = new();
        _listToFileConverters = new();
        _dateTimeServiceMock = new();
        _accountActivityFactoryMock = new();
        _activityFactoriesMock = new();
        _userServiceMock = new();
        _naicsServiceMock = new();

        _csvMock.Setup(x => x.FileType).Returns(FileType.Csv);
        _xlsxMock.Setup(x => x.FileType).Returns(FileType.Xlsx);

        _listToFileConverters
            .Setup(x => x.GetEnumerator())
            .Returns(new[] { _csvMock.Object, _xlsxMock.Object }.ToList()
                .GetEnumerator());

        _accountActivityFactoryMock
            .Setup(x => x.EventType)
            .Returns(AccountEventType.AccountCreated);

        _accountActivityFactoryMock
            .Setup(x => x.Revision)
            .Returns(1);

        _activityFactoriesMock
            .Setup(x => x.GetEnumerator())
            .Returns(new[] { _accountActivityFactoryMock.Object }.ToList().GetEnumerator());

        _dbContextMock = new TaxBeaconDbContext(
            new DbContextOptionsBuilder<TaxBeaconDbContext>()
                .UseInMemoryDatabase($"{nameof(AccountsServiceTests)}-InMemoryDb-{Guid.NewGuid()}")
                .Options,
            _entitySaveChangesInterceptorMock.Object);

        _accountService = new AccountService(
            _accountServiceLoggerMock.Object,
            _dbContextMock,
            _dbContextMock,
            _currentUserServiceMock.Object,
            _dateTimeServiceMock.Object,
            _listToFileConverters.Object,
            _activityFactoriesMock.Object,
            _userServiceMock.Object,
            _naicsServiceMock.Object);

        TypeAdapterConfig.GlobalSettings.Scan(typeof(IAccountService).Assembly);
    }

    private static class TestData
    {
        public static Faker<AccountView> AccountsViewFaker =>
            new Faker<AccountView>()
                .RuleFor(a => a.Id, _ => Guid.NewGuid())
                .RuleFor(a => a.Name, f => f.Company.CompanyName())
                .RuleFor(a => a.AccountId, f => f.Random.String(10))
                .RuleFor(a => a.CreatedDateTimeUtc, _ => DateTime.UtcNow)
                .RuleFor(a => a.State, f => f.PickRandom<State>())
                .RuleFor(a => a.City, f => f.Address.City())
                .RuleFor(a => a.Website, f => f.Internet.Url())
                .RuleFor(a => a.AccountType, f => f.Name.JobTitle())
                .RuleFor(a => a.Country, f => f.PickRandom(Country.UnitedStates.Name, Country.International.Name));

        public static Faker<Account> AccountsFaker =>
            new Faker<Account>()
                .RuleFor(a => a.Id, _ => Guid.NewGuid())
                .RuleFor(a => a.Name, f => f.Company.CompanyName())
                .RuleFor(a => a.AccountId, f => f.Random.String(10))
                .RuleFor(a => a.CreatedDateTimeUtc, _ => DateTime.UtcNow)
                .RuleFor(a => a.Website, f => f.Internet.Url())
                .RuleFor(a => a.Country, f => f.Address.Country())
                .RuleFor(a => a.State, f => f.PickRandom<State>())
                .RuleFor(a => a.City, f => f.Address.City())
                .RuleFor(a => a.Address1, f => f.Address.StreetAddress())
                .RuleFor(a => a.Address2, f => f.Address.StreetAddress())
                .RuleFor(a => a.Zip, f => f.Random.Number(10000, 9999999).ToString())
                .RuleFor(a => a.County, f => f.Address.County())
                .RuleFor(a => a.LastModifiedDateTimeUtc, f => f.Date.Past(1, DateTime.UtcNow));

        public static Faker<Client> ClientsFaker =>
            new Faker<Client>()
                .RuleFor(a => a.Account, _ => AccountsFaker.Generate())
                .RuleFor(a => a.CreatedDateTimeUtc, _ => DateTime.UtcNow)
                .RuleFor(a => a.ReactivationDateTimeUtc, _ => DateTime.UtcNow)
                .RuleFor(a => a.DeactivationDateTimeUtc, _ => DateTime.UtcNow)
                .RuleFor(a => a.AnnualRevenue, f => f.Finance.Amount())
                .RuleFor(a => a.FoundationYear, f => f.Random.Number(1900, 2023))
                .RuleFor(a => a.State, f => f.PickRandom("Client", "Client prospect"))
                .RuleFor(a => a.EmployeeCount, f => f.Random.Number(0, 1000))
                .RuleFor(a => a.Status, f => f.PickRandom<Status>())
                .RuleFor(a => a.DaysOpen, f => f.Random.Number(0, 1000))
                .RuleFor(a => a.LastModifiedDateTimeUtc, f => f.Date.Past(1, DateTime.UtcNow));

        public static Faker<ClientView> ClientsViewFaker =>
            new Faker<ClientView>()
                .RuleFor(a => a.CreatedDateTimeUtc, _ => DateTime.UtcNow)
                .RuleFor(a => a.AccountId, f => f.Random.Guid())
                .RuleFor(a => a.AccountIdField, f => f.Random.String(10))
                .RuleFor(a => a.ClientState, f => f.PickRandom("Client", "Client prospect"))
                .RuleFor(a => a.Status, f => f.PickRandom<Status>())
                .RuleFor(a => a.DaysOpen, f => f.Random.Number(0, 1000))
                .RuleFor(a => a.Name, f => f.Company.CompanyName())
                .RuleFor(a => a.Country, f => f.PickRandom(Country.UnitedStates.Name, Country.International.Name));

        public static Faker<ReferralView> ReferralsViewFaker =>
    new Faker<ReferralView>()
        .RuleFor(a => a.CreatedDateTimeUtc, _ => DateTime.UtcNow)
        .RuleFor(a => a.AccountId, f => f.Random.Guid())
        .RuleFor(a => a.AccountIdField, f => f.Random.String(10))
        .RuleFor(a => a.ReferralState, f => f.PickRandom("Referral prospect", "Referral partner"))
        .RuleFor(a => a.Status, f => f.PickRandom<Status>())
        .RuleFor(a => a.DaysOpen, f => f.Random.Number(0, 1000))
        .RuleFor(a => a.Name, f => f.Company.CompanyName())
        .RuleFor(a => a.Country, f => f.PickRandom(Country.UnitedStates.Name, Country.International.Name));

        public static Faker<Referral> ReferralsFaker =>
    new Faker<Referral>()
        .RuleFor(a => a.Account, _ => AccountsFaker.Generate())
        .RuleFor(a => a.CreatedDateTimeUtc, _ => DateTime.UtcNow)
        .RuleFor(a => a.ReactivationDateTimeUtc, _ => DateTime.UtcNow)
        .RuleFor(a => a.DeactivationDateTimeUtc, _ => DateTime.UtcNow)
        .RuleFor(a => a.State, f => f.PickRandom("Referral prospect", "Referral partner"))
        .RuleFor(a => a.Type, f => f.PickRandom("Paid", "Not Paid"))
        .RuleFor(a => a.OrganizationType, f => f.PickRandom("Employee", "Client", "Contractor", "Third Party"))
        .RuleFor(a => a.Status, f => f.PickRandom<Status>())
        .RuleFor(a => a.DaysOpen, f => f.Random.Number(0, 1000))
        .RuleFor(a => a.LastModifiedDateTimeUtc, f => f.Date.Past(1, DateTime.UtcNow));

        public static Faker<UpdateClientDto> UpdateClientDtoFaker =>
            new Faker<UpdateClientDto>()
            .RuleFor(a => a.AnnualRevenue, f => f.Finance.Amount())
            .RuleFor(a => a.EmployeeCount, f => f.Random.Number(0, 1000))
            .RuleFor(a => a.FoundationYear, f => f.Random.Number(1900, 2023));

        public static Faker<CreateClientDto> CreateClientDtoFaker =>
            new Faker<CreateClientDto>()
            .RuleFor(a => a.AnnualRevenue, f => f.Finance.Amount())
            .RuleFor(a => a.EmployeeCount, f => f.Random.Number(0, 1000))
            .RuleFor(a => a.FoundationYear, f => f.Random.Number(1900, 2023))
            .RuleFor(dto => dto.ClientManagersIds, f => f.Make(3, Guid.NewGuid));

        public static Faker<Tenant> TenantFaker =>
            new Faker<Tenant>()
                .RuleFor(t => t.Id, _ => Guid.NewGuid())
                .RuleFor(t => t.Name, f => f.Company.CompanyName())
                .RuleFor(t => t.CreatedDateTimeUtc, _ => DateTime.UtcNow);

        public static Faker<AccountPhone> PhoneFaker =>
            new Faker<AccountPhone>()
                .RuleFor(p => p.Id, _ => Guid.NewGuid())
                .RuleFor(p => p.Number, f => f.Phone.PhoneNumber())
                .RuleFor(p => p.Type, f => f.PickRandom<PhoneType>());

        public static Faker<CreateUpdatePhoneDto> CreateUpdatePhoneDtoFaker =>
            new Faker<CreateUpdatePhoneDto>()
                .RuleFor(p => p.Number, f => f.Phone.PhoneNumber())
                .RuleFor(p => p.Type, f => f.PickRandom<PhoneType>())
                .RuleFor(p => p.Extension, f => f.PickRandom(1, 20).ToString());

        public static Faker<User> UserFaker =>
            new Faker<User>()
                .RuleFor(u => u.Id, _ => Guid.NewGuid())
                .RuleFor(u => u.FirstName, f => f.Name.FirstName())
                .RuleFor(u => u.LastName, f => f.Name.LastName())
                .RuleFor(u => u.LegalName, (_, u) => u.FirstName)
                .RuleFor(u => u.FullName, (_, u) => $"{u.FirstName} {u.LastName}")
                .RuleFor(u => u.Email, f => f.Internet.Email())
                .RuleFor(u => u.CreatedDateTimeUtc, _ => DateTime.UtcNow)
                .RuleFor(u => u.Status, f => f.PickRandom<Status>());

        public static Faker<UpdateAccountProfileDto> UpdateAccountProfileDtoFaker =>
            new Faker<UpdateAccountProfileDto>()
                .RuleFor(a => a.Name, f => f.Company.CompanyName())
                .RuleFor(a => a.Website, f => f.Internet.Url())
                .RuleFor(a => a.Country, f => f.Address.Country())
                .RuleFor(a => a.State, f => f.PickRandom<State>())
                .RuleFor(a => a.City, f => f.Address.City())
                .RuleFor(a => a.Address1, f => f.Address.StreetAddress())
                .RuleFor(a => a.Address2, f => f.Address.StreetAddress())
                .RuleFor(a => a.Zip, f => f.Random.Number(10000, 9999999).ToString())
                .RuleFor(a => a.County, f => f.Address.County());

        public static Faker<CreateAccountDto> CreateAccountDtoFaker =>
            new Faker<CreateAccountDto>()
                .RuleFor(a => a.Name, f => f.Company.CompanyName())
                .RuleFor(a => a.AccountId, f => f.Random.String(10))
                .RuleFor(a => a.Website, f => f.Internet.Url())
                .RuleFor(a => a.Country, f => f.Address.Country())
                .RuleFor(a => a.State, f => f.PickRandom<State>())
                .RuleFor(a => a.City, f => f.Address.City())
                .RuleFor(a => a.Address1, f => f.Address.StreetAddress())
                .RuleFor(a => a.Address2, f => f.Address.StreetAddress())
                .RuleFor(a => a.Zip, f => f.Random.Number(10000, 9999999).ToString())
                .RuleFor(a => a.County, f => f.Address.County());
    }
}
