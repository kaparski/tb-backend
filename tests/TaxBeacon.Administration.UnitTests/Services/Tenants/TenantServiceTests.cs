using Bogus;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using TaxBeacon.Common.Converters;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL;
using TaxBeacon.DAL.Interceptors;
using TaxBeacon.Administration.Tenants;
using TaxBeacon.Administration.Tenants.Activities;
using TaxBeacon.Administration.Tenants.Models;
using TaxBeacon.DAL.Administration;
using TaxBeacon.DAL.Administration.Entities;

namespace TaxBeacon.Administration.UnitTests.Services.Tenants;

public partial class TenantServiceTests
{
    private readonly Mock<IDateTimeService> _dateTimeServiceMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IListToFileConverter> _csvMock;
    private readonly Mock<IListToFileConverter> _xlsxMock;
    private readonly ITaxBeaconDbContext _dbContextMock;
    private readonly TenantService _tenantService;

    public TenantServiceTests()
    {
        Mock<ILogger<TenantService>> tenantServiceLoggerMock = new();
        Mock<EntitySaveChangesInterceptor> entitySaveChangesInterceptorMock = new();
        Mock<IEnumerable<IListToFileConverter>> listToFileConverters = new();
        Mock<IDateTimeFormatter> dateTimeFormatterMock = new();
        Mock<IEnumerable<ITenantActivityFactory>> activityFactoriesMock = new();

        _currentUserServiceMock = new();
        _dateTimeServiceMock = new();
        _csvMock = new();
        _xlsxMock = new();

        _csvMock.Setup(x => x.FileType).Returns(FileType.Csv);
        _xlsxMock.Setup(x => x.FileType).Returns(FileType.Xlsx);

        listToFileConverters
            .Setup(x => x.GetEnumerator())
            .Returns(new[] { _csvMock.Object, _xlsxMock.Object }.ToList()
                .GetEnumerator());

        _dbContextMock = new TaxBeaconDbContext(
            new DbContextOptionsBuilder<TaxBeaconDbContext>()
                .UseInMemoryDatabase($"{nameof(TenantServiceTests)}-InMemoryDb-{Guid.NewGuid()}")
                .Options,
            entitySaveChangesInterceptorMock.Object);

        activityFactoriesMock
            .Setup(x => x.GetEnumerator())
            .Returns(new ITenantActivityFactory[]
            {
                new TenantEnteredEventFactory(), new TenantExitedEventFactory(), new TenantUpdatedEventFactory()
            }.ToList().GetEnumerator());

        _tenantService = new TenantService(
            tenantServiceLoggerMock.Object,
            _dbContextMock,
            _dateTimeServiceMock.Object,
            _currentUserServiceMock.Object,
            listToFileConverters.Object,
            dateTimeFormatterMock.Object,
            activityFactoriesMock.Object);

        TypeAdapterConfig.GlobalSettings.Scan(typeof(ITenantService).Assembly);
    }

    private static class TestData
    {
        public static readonly Guid TestTenantId = Guid.NewGuid();

        public static Faker<Tenant> TenantFaker =>
            new Faker<Tenant>()
                .RuleFor(t => t.Id, _ => Guid.NewGuid())
                .RuleFor(t => t.Name, f => f.Company.CompanyName())
                .RuleFor(t => t.CreatedDateTimeUtc, _ => DateTime.UtcNow)
                .RuleFor(t => t.TenantsPrograms, _ => ProgramFaker.Generate(3)
                    .Select(p => new TenantProgram { Program = p, IsDeleted = false }).ToList());

        public static Faker<User> UserFaker =>
            new Faker<User>()
                .RuleFor(u => u.Id, _ => Guid.NewGuid())
                .RuleFor(u => u.FirstName, f => f.Name.FirstName())
                .RuleFor(u => u.LastName, f => f.Name.LastName())
                .RuleFor(u => u.FullName, (_, u) => $"{u.FirstName} {u.LastName}")
                .RuleFor(u => u.LegalName, (_, u) => u.FirstName)
                .RuleFor(u => u.Email, f => f.Internet.Email())
                .RuleFor(u => u.CreatedDateTimeUtc, _ => DateTime.UtcNow)
                .RuleFor(u => u.Status, f => f.PickRandom<Status>());

        public static Faker<UpdateTenantDto> UpdateTenantDtoFaker =>
            new Faker<UpdateTenantDto>()
                .CustomInstantiator(f => new UpdateTenantDto(f.Company.CompanyName()));

        public static Faker<Program> ProgramFaker => new Faker<Program>()
            .RuleFor(p => p.Id, _ => Guid.NewGuid())
            .RuleFor(p => p.Name, f => f.Name.FirstName());
    }
}
