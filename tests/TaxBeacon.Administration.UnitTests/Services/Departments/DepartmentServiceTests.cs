using Mapster;
using Microsoft.EntityFrameworkCore;
using TaxBeacon.Administration.Departments.Activities.Factories;
using TaxBeacon.Administration.Departments;
using TaxBeacon.Common.Converters;
using TaxBeacon.Common.Enums;
using TaxBeacon.DAL;
using Microsoft.Extensions.Logging;
using Moq;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL.Administration;
using TaxBeacon.DAL.Interceptors;
using Bogus;
using TaxBeacon.Administration.Departments.Models;
using TaxBeacon.DAL.Administration.Entities;

namespace TaxBeacon.Administration.UnitTests.Services.Departments;
public partial class DepartmentServiceTests
{
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IDateTimeService> _dateTimeServiceMock;
    private readonly Mock<IListToFileConverter> _csvMock;
    private readonly Mock<IListToFileConverter> _xlsxMock;
    private readonly ITaxBeaconDbContext _dbContextMock;
    private readonly DepartmentService _departmentService;

    public DepartmentServiceTests()
    {
        Mock<ILogger<DepartmentService>> departmentServiceLoggerMock = new();
        Mock<EntitySaveChangesInterceptor> entitySaveChangesInterceptorMock = new();
        Mock<IEnumerable<IListToFileConverter>> listToFileConverters = new();
        Mock<IDateTimeFormatter> dateTimeFormatterMock = new();
        Mock<IEnumerable<IDepartmentActivityFactory>> activityFactoriesMock = new();

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
                .UseInMemoryDatabase($"{nameof(DepartmentServiceTests)}-InMemoryDb-{Guid.NewGuid()}")
                .Options,
        entitySaveChangesInterceptorMock.Object);

        var currentUser = TestData.TestUser.Generate();
        _dbContextMock.Users.Add(currentUser);
        _dbContextMock.SaveChangesAsync().Wait();
        _currentUserServiceMock.Setup(x => x.UserId).Returns(currentUser.Id);
        _currentUserServiceMock.Setup(x => x.TenantId).Returns(TestData.TestTenantId);

        activityFactoriesMock
            .Setup(x => x.GetEnumerator())
            .Returns(new IDepartmentActivityFactory[]
            {
                new DepartmentUpdatedEventFactory()
            }.ToList().GetEnumerator());

        _departmentService = new DepartmentService(
            departmentServiceLoggerMock.Object,
            _dbContextMock,
            _dateTimeServiceMock.Object,
            _currentUserServiceMock.Object,
            listToFileConverters.Object,
            dateTimeFormatterMock.Object,
            activityFactoriesMock.Object);

        TypeAdapterConfig.GlobalSettings.Scan(typeof(IDepartmentService).Assembly);
    }

    private static class TestData
    {
        public static readonly Guid TestTenantId = Guid.NewGuid();

        public static readonly Faker<Tenant> TestTenant =
            new Faker<Tenant>()
                .RuleFor(t => t.Id, f => TestTenantId)
                .RuleFor(t => t.Name, f => f.Company.CompanyName())
                .RuleFor(t => t.CreatedDateTimeUtc, f => DateTime.UtcNow);

        public static readonly Faker<ServiceArea> TestServiceArea =
            new Faker<ServiceArea>()
                .RuleFor(t => t.Id, f => Guid.NewGuid())
                .RuleFor(t => t.Name, f => f.Company.CompanyName())
                .RuleFor(t => t.CreatedDateTimeUtc, f => DateTime.UtcNow);

        public static readonly Faker<Department> TestDepartment =
            new Faker<Department>()
                .RuleFor(t => t.Id, f => Guid.NewGuid())
                .RuleFor(t => t.TenantId, f => TestTenantId)
                .RuleFor(t => t.Name, f => f.Company.CompanyName())
                .RuleFor(t => t.Description, f => f.Lorem.Sentence(2))
                .RuleFor(t => t.CreatedDateTimeUtc, f => DateTime.UtcNow);

        public static readonly Faker<User> TestUser =
            new Faker<User>()
                .RuleFor(u => u.Id, f => Guid.NewGuid())
                .RuleFor(u => u.FirstName, f => f.Name.FirstName())
                .RuleFor(u => u.LastName, f => f.Name.LastName())
                .RuleFor(u => u.FullName, (_, u) => $"{u.FirstName} {u.LastName}")
                .RuleFor(u => u.LegalName, (_, u) => u.FirstName)
                .RuleFor(u => u.Email, f => f.Internet.Email())
                .RuleFor(u => u.CreatedDateTimeUtc, f => DateTime.UtcNow)
                .RuleFor(u => u.Status, f => f.PickRandom<Status>())
                .RuleFor(u => u.Team, f =>
                    new Team { Name = f.Name.FirstName() })
                .RuleFor(u => u.JobTitle, f =>
                    new JobTitle { Name = f.Name.FirstName() });

        public static readonly Faker<Division> TestDivision =
            new Faker<Division>()
                .RuleFor(d => d.Id, _ => Guid.NewGuid())
                .RuleFor(d => d.Name, f => f.Name.JobType())
                .RuleFor(d => d.CreatedDateTimeUtc, _ => DateTime.UtcNow)
                .RuleFor(d => d.Description, f => f.Lorem.Sentence(2))
                .RuleFor(d => d.TenantId, _ => TestTenantId);

        public static readonly Faker<UpdateDepartmentDto> UpdateDepartmentDtoFaker =
            new Faker<UpdateDepartmentDto>()
                .RuleFor(dto => dto.Name, f => f.Company.CompanyName())
                .RuleFor(dto => dto.ServiceAreasIds, f => f.Make(3, Guid.NewGuid))
                .RuleFor(dto => dto.JobTitlesIds, f => f.Make(3, Guid.NewGuid))
                .RuleFor(dto => dto.DivisionId, Guid.NewGuid());

        public static readonly Faker<JobTitle> TestJobTitle = new Faker<JobTitle>()
            .RuleFor(jt => jt.Id, f => Guid.NewGuid())
            .RuleFor(jt => jt.Name, f => f.Name.JobTitle())
            .RuleFor(t => t.CreatedDateTimeUtc, f => DateTime.UtcNow);
    }
}
