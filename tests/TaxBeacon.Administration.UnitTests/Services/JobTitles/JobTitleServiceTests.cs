using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using TaxBeacon.Administration.JobTitles.Activities.Factories;
using TaxBeacon.Administration.JobTitles;
using TaxBeacon.Common.Converters;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL.Administration;
using TaxBeacon.DAL.Interceptors;
using TaxBeacon.DAL;
using Bogus;
using TaxBeacon.Administration.JobTitles.Models;
using TaxBeacon.DAL.Administration.Entities;

namespace TaxBeacon.Administration.UnitTests.Services.JobTitles;
public partial class JobTitleServiceTests
{
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IListToFileConverter> _csvMock;
    private readonly Mock<IListToFileConverter> _xlsxMock;
    private readonly Mock<IDateTimeService> _dateTimeServiceMock;
    private readonly ITaxBeaconDbContext _dbContextMock;
    private readonly JobTitleService _serviceAreaService;

    public JobTitleServiceTests()
    {
        Mock<ILogger<JobTitleService>> serviceAreaServiceLoggerMock = new();
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
                .UseInMemoryDatabase($"{nameof(JobTitleServiceTests)}-InMemoryDb-{Guid.NewGuid()}")
                .Options,
            entitySaveChangesInterceptorMock.Object);

        _currentUserServiceMock = new();

        Mock<IEnumerable<IJobTitleActivityFactory>> activityFactoriesMock = new();
        activityFactoriesMock
            .Setup(x => x.GetEnumerator())
            .Returns(new IJobTitleActivityFactory[]
            {
                new JobTitleUpdatedEventFactory()
            }.ToList().GetEnumerator());

        _dateTimeServiceMock = new();

        _serviceAreaService = new JobTitleService(
            serviceAreaServiceLoggerMock.Object,
            _dbContextMock,
            _dateTimeServiceMock.Object,
            _currentUserServiceMock.Object,
            dateTimeFormatterMock.Object,
            listToFileConverters.Object,
            activityFactoriesMock.Object);
    }

    private static class TestData
    {
        public static readonly Guid TestTenantId = Guid.NewGuid();

        public static readonly Faker<JobTitle> TestJobTitle =
            new Faker<JobTitle>()
                .RuleFor(t => t.Id, f => Guid.NewGuid())
                .RuleFor(t => t.Name, f => f.Company.CompanyName())
                .RuleFor(t => t.CreatedDateTimeUtc, f => DateTime.UtcNow)
                .RuleFor(t => t.TenantId, f => TestTenantId);

        public static readonly Faker<User> TestUser =
            new Faker<User>()
                .RuleFor(u => u.Id, f => Guid.NewGuid())
                .RuleFor(u => u.FirstName, f => f.Name.FirstName())
                .RuleFor(u => u.LastName, f => f.Name.LastName())
                .RuleFor(u => u.FullName, (_, u) => $"{u.FirstName} {u.LastName}")
                .RuleFor(u => u.LegalName, (_, u) => u.FirstName)
                .RuleFor(u => u.Email, f => f.Internet.Email())
                .RuleFor(u => u.CreatedDateTimeUtc, f => DateTime.UtcNow)
                .RuleFor(u => u.TenantUsers, f => new List<TenantUser>()
                {
                     new TenantUser()
                     {
                         TenantId = TestTenantId,
                     }
                })
                .RuleFor(u => u.Status, f => f.PickRandom<Status>());

        public static readonly Faker<UpdateJobTitleDto> UpdateJobTitleDtoFaker =
            new Faker<UpdateJobTitleDto>()
                .RuleFor(dto => dto.Name, f => f.Company.CompanyName())
                .RuleFor(dto => dto.Description, f => f.Lorem.Text());
    }
}
