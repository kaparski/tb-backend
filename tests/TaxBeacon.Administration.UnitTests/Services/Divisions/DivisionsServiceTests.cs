using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using TaxBeacon.Administration.Divisions.Activities.Factories;
using TaxBeacon.Administration.Divisions;
using TaxBeacon.Common.Converters;
using TaxBeacon.Common.Enums.Administration.Activities;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL.Administration.Entities;
using TaxBeacon.DAL.Administration;
using TaxBeacon.DAL.Interceptors;
using TaxBeacon.DAL;
using Bogus;
using TaxBeacon.Administration.Divisions.Models;

namespace TaxBeacon.Administration.UnitTests.Services.Divisions;
public partial class DivisionsServiceTests
{
    private readonly DivisionsService _divisionsService;
    private readonly ITaxBeaconDbContext _dbContextMock;
    private readonly Mock<IDateTimeService> _dateTimeServiceMock;
    private readonly Mock<IListToFileConverter> _csvMock;
    private readonly Mock<IListToFileConverter> _xlsxMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IDivisionActivityFactory> _divisionActivityFactory;
    private readonly Mock<IEnumerable<IDivisionActivityFactory>> _activityFactories;

    private readonly User _currentUser = TestData.TestUser.Generate();
    private static readonly Guid TenantId = Guid.NewGuid();
    public DivisionsServiceTests()
    {
        Mock<EntitySaveChangesInterceptor> entitySaveChangesInterceptorMock = new();
        Mock<ILogger<DivisionsService>> divisionsServiceLoggerMock = new();
        Mock<IDateTimeFormatter> dateTimeFormatterMock = new();
        Mock<IEnumerable<IListToFileConverter>> listToFileConverters = new();

        _dateTimeServiceMock = new();
        _csvMock = new();
        _xlsxMock = new();
        _dbContextMock = new TaxBeaconDbContext(
            new DbContextOptionsBuilder<TaxBeaconDbContext>()
                .UseInMemoryDatabase($"{nameof(DivisionsServiceTests)}-InMemoryDb-{Guid.NewGuid()}")
                .Options,
            entitySaveChangesInterceptorMock.Object);

        _csvMock.Setup(x => x.FileType).Returns(FileType.Csv);
        _xlsxMock.Setup(x => x.FileType).Returns(FileType.Xlsx);

        listToFileConverters
            .Setup(x => x.GetEnumerator())
            .Returns(new[]
                {
                    _csvMock.Object, _xlsxMock.Object
                }.ToList()
                .GetEnumerator());
        _currentUserServiceMock = new();
        _dbContextMock.Tenants.Add(new Tenant()
        {
            Id = TenantId,
            Name = "TestTenant",
        });
        _dbContextMock.SaveChangesAsync();
        _currentUserServiceMock.Setup(x => x.TenantId).Returns(TenantId);

        _divisionActivityFactory = new();
        _divisionActivityFactory.Setup(x => x.EventType).Returns(DivisionEventType.None);
        _divisionActivityFactory.Setup(x => x.Revision).Returns(1);

        _activityFactories = new();
        _activityFactories
            .Setup(x => x.GetEnumerator())
            .Returns(new[] { _divisionActivityFactory.Object }.ToList().GetEnumerator());
        _divisionsService = new DivisionsService(divisionsServiceLoggerMock.Object,
            _dbContextMock,
            _dateTimeServiceMock.Object,
            _currentUserServiceMock.Object,
            listToFileConverters.Object,
            dateTimeFormatterMock.Object,
            _activityFactories.Object);

        var currentUser = TestData.TestUser.Generate();
        _dbContextMock.Users.Add(currentUser);
        _dbContextMock.SaveChangesAsync().Wait();
        _currentUserServiceMock.Setup(x => x.UserId).Returns(currentUser.Id);

        _activityFactories
            .Setup(x => x.GetEnumerator())
            .Returns(new IDivisionActivityFactory[]
            {
                new DivisionUpdatedEventFactory(),
            }.ToList().GetEnumerator());
    }

    private static class TestData
    {
        public static readonly Faker<User> TestUser =
            new Faker<User>()
                .RuleFor(u => u.Id, _ => Guid.NewGuid())
                .RuleFor(u => u.FirstName, f => f.Name.FirstName())
                .RuleFor(u => u.LastName, f => f.Name.LastName())
                .RuleFor(u => u.LegalName, (_, u) => u.FirstName)
                .RuleFor(u => u.FullName, (_, u) => $"{u.FirstName} {u.LastName}")
                .RuleFor(u => u.Email, f => f.Internet.Email())
                .RuleFor(u => u.CreatedDateTimeUtc, _ => DateTime.UtcNow)
                .RuleFor(u => u.Status, f => f.PickRandom<Status>())
                .RuleFor(u => u.JobTitle, f =>
                    new JobTitle
                    {
                        Name = f.Name.FirstName()
                    })
                .RuleFor(u => u.Department, f =>
                    new Department
                    {
                        Name = f.Name.FirstName()
                    })
                .RuleFor(u => u.TenantUsers, (f, u) =>
                    new List<TenantUser>()
                    {
                        new TenantUser()
                        {
                            UserId = u.Id,
                            TenantId = TenantId
                        }
                    });

        public static readonly Faker<Division> TestDivision =
            new Faker<Division>()
                .RuleFor(d => d.Id, _ => Guid.NewGuid())
                .RuleFor(d => d.Name, f => f.Name.JobType())
                .RuleFor(d => d.CreatedDateTimeUtc, _ => DateTime.UtcNow)
                .RuleFor(d => d.Description, f => f.Lorem.Sentence(2))
                .RuleFor(d => d.TenantId, _ => DivisionsServiceTests.TenantId)
                .RuleFor(d => d.Departments, f => TestData.TestDepartment.Generate(f.Random.Int(1, 3)));

        public static readonly Faker<Tenant> TestTenant =
            new Faker<Tenant>()
                .RuleFor(t => t.Id, _ => Guid.NewGuid())
                .RuleFor(t => t.Name, f => f.Company.CompanyName())
                .RuleFor(t => t.CreatedDateTimeUtc, _ => DateTime.UtcNow);

        public static readonly Faker<UpdateDivisionDto> UpdateDivisionDtoFaker =
            new Faker<UpdateDivisionDto>()
                .RuleFor(t => t.Name, f => f.Name.JobType())
                .RuleFor(t => t.Description, f => f.Lorem.Sentence(2))
                .RuleFor(t => t.DepartmentIds, f => f.Make(3, Guid.NewGuid));

        public static readonly Faker<Department> TestDepartment =
            new Faker<Department>()
                .RuleFor(t => t.Id, f => Guid.NewGuid())
                .RuleFor(t => t.TenantId, _ => DivisionsServiceTests.TenantId)
                .RuleFor(t => t.Name, f => f.Company.CompanyName())
                .RuleFor(t => t.Description, f => f.Lorem.Sentence(2))
                .RuleFor(t => t.CreatedDateTimeUtc, f => DateTime.UtcNow);
    }
}
