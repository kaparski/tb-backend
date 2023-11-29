
using Bogus;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using TaxBeacon.Accounts.Documents;
using TaxBeacon.Common.Converters;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL.Documents.Entities;
using TaxBeacon.DAL.Accounts.Entities;
using TaxBeacon.DAL.Administration.Entities;
using TaxBeacon.DAL.Interceptors;
using TaxBeacon.DAL;

namespace TaxBeacon.Accounts.UnitTests.Documents;
public partial class DocumentServiceTests
{
    private readonly TaxBeaconDbContext _dbContextMock;
    private readonly DocumentService _documentService;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IDateTimeService> _dateTimeServiceMock;
    private readonly Mock<ILogger<DocumentService>> _documentServiceLoggerMock;
    private readonly Mock<IListToFileConverter> _csvMock = new();
    private readonly Mock<IListToFileConverter> _xlsxMock = new();
    private readonly Mock<IEnumerable<IListToFileConverter>> _listToFileConverters = new();
    private readonly Mock<IDateTimeFormatter> _dateTimeFormatterMock = new();

    public DocumentServiceTests()
    {
        _currentUserServiceMock = new();
        _dateTimeServiceMock = new();
        _documentServiceLoggerMock = new();
        Mock<EntitySaveChangesInterceptor> entitySaveChangesInterceptorMock = new();
        _dbContextMock = new TaxBeaconDbContext(
            new DbContextOptionsBuilder<TaxBeaconDbContext>()
                .UseInMemoryDatabase($"{nameof(DocumentServiceTests)}-InMemoryDb-{Guid.NewGuid()}")
                .Options,
            entitySaveChangesInterceptorMock.Object);

        _csvMock.Setup(x => x.FileType).Returns(FileType.Csv);
        _xlsxMock.Setup(x => x.FileType).Returns(FileType.Xlsx);
        _listToFileConverters
            .Setup(x => x.GetEnumerator())
            .Returns(new[] { _csvMock.Object, _xlsxMock.Object }.ToList()
                .GetEnumerator());

        _documentService = new DocumentService(
            _dbContextMock,
            _currentUserServiceMock.Object,
            _dateTimeFormatterMock.Object,
            _documentServiceLoggerMock.Object,
            _dateTimeServiceMock.Object,
            _listToFileConverters.Object
            );

        TypeAdapterConfig.GlobalSettings.Scan(typeof(IDocumentService).Assembly);
    }

    private static class TestData
    {
        public static Faker<Document> DocumentFaker =>
            new Faker<Document>()
                .RuleFor(d => d.Id, f => Guid.NewGuid())
                .RuleFor(d => d.Name, f => f.Lorem.Word())
                .RuleFor(d => d.Url, f => f.Lorem.Word())
                .RuleFor(d => d.CreatedDateTimeUtc, f => DateTime.UtcNow)
                .RuleFor(d => d.ContentLength, f => f.Random.Int(1, 1000000000));

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
    }
}
