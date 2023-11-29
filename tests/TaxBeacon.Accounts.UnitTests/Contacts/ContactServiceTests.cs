using Bogus;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;
using TaxBeacon.Accounts.Common.Models;
using TaxBeacon.Accounts.Contacts;
using TaxBeacon.Accounts.Contacts.Activities.Factories;
using TaxBeacon.Accounts.Contacts.Activities.Models;
using TaxBeacon.Accounts.Contacts.Models;
using TaxBeacon.Common.Converters;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Enums.Accounts;
using TaxBeacon.Common.Enums.Accounts.Activities;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL;
using TaxBeacon.DAL.Accounts.Entities;
using TaxBeacon.DAL.Administration.Entities;
using TaxBeacon.DAL.Interceptors;

namespace TaxBeacon.Accounts.UnitTests.Contacts;

public partial class ContactServiceTests
{
    private readonly Mock<IDateTimeService> _dateTimeServiceMock;
    private readonly TaxBeaconDbContext _dbContext;
    private readonly IContactService _contactService;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IListToFileConverter> _csvMock;
    private readonly Mock<IListToFileConverter> _xlsxMock;
    private readonly Mock<IEnumerable<IActivityFactory<ContactEventType>>> _activityFactoriesMock = new();

    public ContactServiceTests()
    {
        Mock<ILogger<ContactService>> loggerMock = new();
        _dateTimeServiceMock = new();
        _currentUserServiceMock = new();

        _csvMock = new();
        _xlsxMock = new();
        Mock<IEnumerable<IListToFileConverter>> listToFileConverters = new();

        _csvMock.Setup(x => x.FileType).Returns(FileType.Csv);
        _xlsxMock.Setup(x => x.FileType).Returns(FileType.Xlsx);

        listToFileConverters
            .Setup(x => x.GetEnumerator())
            .Returns(new[] { _csvMock.Object, _xlsxMock.Object }.ToList()
                .GetEnumerator());

        Mock<EntitySaveChangesInterceptor> entitySaveChangesInterceptorMock = new();
        _dbContext = new TaxBeaconDbContext(
            new DbContextOptionsBuilder<TaxBeaconDbContext>()
                .UseInMemoryDatabase($"{nameof(ContactServiceTests)}-InMemoryDb-{Guid.NewGuid()}")
                .Options,
            entitySaveChangesInterceptorMock.Object);

        _activityFactoriesMock
            .Setup(x => x.GetEnumerator())
            .Returns(new IActivityFactory<ContactEventType>[]
            {
                new ContactCreatedEventFactory(), new ContactDeactivatedEventFactory(),
                new ContactReactivatedEventFactory(), new ContactAssignedToAccountEventFactory()
            }.ToList().GetEnumerator());

        _contactService = new ContactService(loggerMock.Object,
            _currentUserServiceMock.Object,
            _dbContext,
            _dateTimeServiceMock.Object,
            listToFileConverters.Object,
            _activityFactoriesMock.Object);

        TypeAdapterConfig.GlobalSettings.Scan(typeof(IContactService).Assembly);
    }

    private static class TestData
    {
        public static Faker<Contact> ContactFaker =>
            new Faker<Contact>()
                .RuleFor(t => t.Id, _ => Guid.NewGuid())
                .RuleFor(t => t.FirstName, f => f.Person.FirstName)
                .RuleFor(t => t.LastName, f => f.Person.LastName)
                .RuleFor(t => t.Email, t => t.Person.Email)
                .RuleFor(t => t.CreatedDateTimeUtc, _ => DateTime.UtcNow);

        public static Faker<Tenant> TenantFaker =>
            new Faker<Tenant>()
                .RuleFor(t => t.Id, _ => Guid.NewGuid())
                .RuleFor(t => t.Name, f => f.Company.CompanyName())
                .RuleFor(t => t.CreatedDateTimeUtc, _ => DateTime.UtcNow);

        public static Faker<LinkedContact> LinkedContactFaker =>
            new Faker<LinkedContact>()
                .RuleFor(t => t.Comment, f => f.Lorem.Paragraph());

        public static Faker<Account> AccountFaker =>
            new Faker<Account>()
                .RuleFor(a => a.Id, _ => Guid.NewGuid())
                .RuleFor(a => a.Website, f => f.Internet.Url())
                .RuleFor(a => a.Name, f => f.Person.FullName)
                .RuleFor(a => a.AccountId, f => f.Random.String(10))
                .RuleFor(a => a.Country, f => f.Address.Country());

        private static Faker<CreateUpdatePhoneDto> CreateUpdatePhoneDtoFaker =>
            new Faker<CreateUpdatePhoneDto>()
                .RuleFor(p => p.Number, f => f.Phone.PhoneNumber())
                .RuleFor(p => p.Type, f => f.PickRandom<PhoneType>())
                .RuleFor(p => p.Extension, f => f.PickRandom(1, 20).ToString());

        public static Faker<CreateContactDto> CreateContactDtoFaker =>
            new Faker<CreateContactDto>()
                .RuleFor(c => c.FirstName, f => f.Person.FirstName)
                .RuleFor(c => c.LastName, f => f.Person.LastName)
                .RuleFor(c => c.Email, f => f.Person.Email)
                .RuleFor(c => c.SecondaryEmail, f => f.Person.Email)
                .RuleFor(c => c.JobTitle, f => f.Person.Company.Name)
                .RuleFor(c => c.Type, f =>
                {
                    var list = ContactType.List.Except(new[] { ContactType.None });
                    return f.PickRandom(list);
                })
                .RuleFor(e => e.Phones, _ => CreateUpdatePhoneDtoFaker.Generate(2));

        public static readonly Faker<ContactActivityLog> ContactActivityFaker =
            new Faker<ContactActivityLog>()
                .RuleFor(l => l.Date, f => f.Date.Recent())
                .RuleFor(l => l.Revision, _ => 1u)
                .RuleFor(l => l.Event, (f, _) => JsonSerializer.Serialize(
                    new ContactCreatedEvent(Guid.NewGuid(), f.Name.JobTitle(), f.Name.FullName(), DateTime.Now)
                ))
                .RuleFor(l => l.EventType, _ => ContactEventType.ContactCreated);

        public static readonly Faker<AccountContactActivityLog> AccountContactActivityFaker =
            new Faker<AccountContactActivityLog>()
                .RuleFor(l => l.Date, f => f.Date.Recent())
                .RuleFor(l => l.Revision, _ => 1u)
                .RuleFor(l => l.Event, (f, _) => JsonSerializer.Serialize(
                    new ContactDeactivatedEvent(Guid.NewGuid(), f.Name.JobTitle(), f.Name.FullName(), DateTime.Now)
                ))
                .RuleFor(l => l.EventType, _ => ContactEventType.ContactDeactivated);

        public static Faker<UpdateContactDto> UpdateContactFaker =>
            new Faker<UpdateContactDto>()
                .RuleFor(c => c.FirstName, f => f.Person.FirstName)
                .RuleFor(c => c.LastName, f => f.Person.LastName)
                .RuleFor(c => c.Email, f => f.Person.Email)
                .RuleFor(c => c.SecondaryEmail, f => f.Person.Email)
                .RuleFor(c => c.JobTitle, f => f.Person.Company.Name);

        public static Faker<UpdateAccountContactDto> UpdateAccountContactFaker =>
            new Faker<UpdateAccountContactDto>()
                .RuleFor(c => c.FirstName, f => f.Person.FirstName)
                .RuleFor(c => c.LastName, f => f.Person.LastName)
                .RuleFor(c => c.Email, f => f.Person.Email)
                .RuleFor(c => c.SecondaryEmail, f => f.Person.Email)
                .RuleFor(c => c.JobTitle, f => f.Person.Company.Name)
                .RuleFor(c => c.Type, f =>
                {
                    var list = ContactType.List.Except(new[]
                    {
                        ContactType.None
                    });
                    return f.PickRandom(list);
                });
    }
}
