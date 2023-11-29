using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using TaxBeacon.Administration.Programs.Activities;
using TaxBeacon.Administration.Programs;
using TaxBeacon.Common.Converters;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL.Administration.Entities;
using TaxBeacon.DAL.Administration;
using TaxBeacon.DAL.Interceptors;
using TaxBeacon.DAL;
using Bogus;
using TaxBeacon.Administration.Programs.Models;
using TaxBeacon.Common.Enums.Administration;
using TaxBeacon.Common.Enums.Administration.Activities;
using System.Text.Json;
using TaxBeacon.Administration.Programs.Activities.Models;

namespace TaxBeacon.Administration.UnitTests.Services.Programs;
public partial class ProgramServiceTests
{
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IListToFileConverter> _csvMock;
    private readonly Mock<IListToFileConverter> _xlsxMock;
    private readonly Mock<IDateTimeService> _dateTimeServiceMock;
    private readonly ITaxBeaconDbContext _dbContextMock;
    private readonly ProgramService _programService;
    private static readonly Guid TenantId = Guid.NewGuid();

    public ProgramServiceTests()
    {
        Mock<ILogger<ProgramService>> programServiceLoggerMock = new();
        Mock<EntitySaveChangesInterceptor> entitySaveChangesInterceptorMock = new();
        Mock<IDateTimeFormatter> dateTimeFormatterMock = new();
        Mock<IEnumerable<IListToFileConverter>> listToFileConverters = new();

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
                .UseInMemoryDatabase($"{nameof(ProgramServiceTests)}-InMemoryDb-{Guid.NewGuid()}")
                .Options,
            entitySaveChangesInterceptorMock.Object);

        _currentUserServiceMock = new();
        _dbContextMock.Tenants.Add(new Tenant()
        {
            Id = TenantId,
            Name = "TestTenant",
        });
        _dbContextMock.SaveChangesAsync();
        _currentUserServiceMock.Setup(x => x.TenantId).Returns(TenantId);

        _dateTimeServiceMock = new();

        Mock<IEnumerable<IProgramActivityFactory>> activityFactoriesMock = new();
        activityFactoriesMock
            .Setup(x => x.GetEnumerator())
            .Returns(new IProgramActivityFactory[]
            {
                new ProgramCreatedEventFactory(),
                new ProgramDeactivatedEventFactory(),
                new ProgramReactivatedEventFactory(),
                new ProgramUpdatedEventFactory(),
            }.ToList().GetEnumerator());

        _programService = new ProgramService(
            programServiceLoggerMock.Object,
            _dbContextMock,
            _dateTimeServiceMock.Object,
            _currentUserServiceMock.Object,
            listToFileConverters.Object,
            dateTimeFormatterMock.Object,
            activityFactoriesMock.Object);
    }

    private static class TestData
    {
        public static readonly Faker<Tenant> TenantFaker =
            new Faker<Tenant>()
                .RuleFor(t => t.Id, _ => Guid.NewGuid())
                .RuleFor(t => t.Name, f => f.Company.CompanyName())
                .RuleFor(t => t.CreatedDateTimeUtc, _ => DateTime.UtcNow);

        public static IEnumerable<object[]> UpdatedStatusInvalidData =>
            new List<object[]>
            {
                new object[] { Status.Active, Guid.NewGuid() }, new object[] { Status.Deactivated, Guid.Empty }
            };

        public static readonly Faker<Program> ProgramFaker = new Faker<Program>()
            .RuleFor(p => p.Id, _ => Guid.NewGuid())
            .RuleFor(p => p.Name, f => f.Name.FirstName());

        public static readonly Faker<TenantProgram> TenantProgramFaker = new Faker<TenantProgram>()
            .RuleFor(p => p.TenantId, _ => TenantId)
            .RuleFor(p => p.Status, f => f.PickRandom<Status>())
            .RuleFor(u => u.ReactivationDateTimeUtc, _ => DateTime.UtcNow)
            .RuleFor(u => u.DeactivationDateTimeUtc, _ => DateTime.UtcNow)
            .RuleFor(p => p.Program, _ => ProgramFaker.Generate())
            .RuleFor(p => p.IsDeleted, _ => false);

        public static readonly Faker<ProgramActivityLog> ProgramActivityLogFaker = new Faker<ProgramActivityLog>()
            .RuleFor(x => x.Date, f => f.Date.Recent())
            .RuleFor(x => x.Revision, _ => (uint)1)
            .RuleFor(x => x.EventType, _ => ProgramEventType.ProgramCreatedEvent)
            .RuleFor(x => x.Event, (f, x) => JsonSerializer.Serialize(
                new ProgramCreatedEvent(Guid.NewGuid(), x.Date, f.Name.FullName(), f.Name.JobTitle())
            ));

        public static readonly Faker<UpdateProgramDto> UpdateProgramDtoFaker =
            new Faker<UpdateProgramDto>()
                .CustomInstantiator(f => new UpdateProgramDto(
                    f.Lorem.Word(),
                    f.Company.CompanyName(),
                    f.Lorem.Text(),
                    f.Lorem.Word(),
                    f.Internet.Url(),
                    f.PickRandom<Jurisdiction>(),
                    f.Address.State(),
                    f.Address.Country(),
                    f.Address.City(),
                    f.Lorem.Word(),
                    f.Lorem.Word(),
                    f.Date.Past(),
                    f.Date.Past()));

        public static readonly Faker<CreateProgramDto> CreateProgramDtoFaker =
            new Faker<CreateProgramDto>()
                .CustomInstantiator(f => new CreateProgramDto(
                    f.Lorem.Word(),
                    f.Company.CompanyName(),
                    f.Lorem.Text(),
                    f.Lorem.Word(),
                    f.Internet.Url(),
                    f.PickRandom<Jurisdiction>(),
                    f.Address.State(),
                    f.Address.Country(),
                    f.Address.City(),
                    f.Lorem.Word(),
                    f.Lorem.Word(),
                    f.Date.Past(),
                    f.Date.Future()));
    }
}
