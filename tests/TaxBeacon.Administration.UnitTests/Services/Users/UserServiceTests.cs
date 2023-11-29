using Bogus;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using TaxBeacon.Administration.Users;
using TaxBeacon.Administration.Users.Activities.Factories;
using TaxBeacon.Administration.Users.Models;
using TaxBeacon.Common.Converters;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Enums.Administration.Activities;
using TaxBeacon.Common.Options;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL;
using TaxBeacon.DAL.Administration;
using TaxBeacon.DAL.Administration.Entities;
using TaxBeacon.DAL.Interceptors;
using TaxBeacon.Email;

namespace TaxBeacon.Administration.UnitTests.Services.Users;

public partial class UserServiceTests
{
    private readonly Mock<IDateTimeService> _dateTimeServiceMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IListToFileConverter> _csvMock;
    private readonly Mock<IListToFileConverter> _xlsxMock;
    private readonly Mock<IUserExternalStore> _userExternalStore;
    private readonly ITaxBeaconDbContext _dbContextMock;
    private readonly UserService _userService;
    private readonly Mock<IUserActivityFactory> _userCreatedActivityFactory;
    private readonly Guid _tenantId = Guid.NewGuid();

    public UserServiceTests()
    {
        Mock<ILogger<UserService>> userServiceLoggerMock = new();
        Mock<EntitySaveChangesInterceptor> entitySaveChangesInterceptorMock = new();
        Mock<IEnumerable<IListToFileConverter>> listToFileConverters = new();
        Mock<IDateTimeFormatter> dateTimeFormatterMock = new();
        Mock<IEnumerable<IUserActivityFactory>> activityFactories = new();
        Mock<IOptionsSnapshot<CreateUserOptions>> createUserOptionsSnapshot = new();
        Mock<IEmailSender> emailSenderMock = new();

        _currentUserServiceMock = new();
        _dateTimeServiceMock = new();
        _userExternalStore = new();
        _csvMock = new();
        _xlsxMock = new();
        _userCreatedActivityFactory = new();

        _csvMock.Setup(x => x.FileType).Returns(FileType.Csv);
        _xlsxMock.Setup(x => x.FileType).Returns(FileType.Xlsx);

        _userCreatedActivityFactory.Setup(x => x.UserEventType).Returns(UserEventType.UserCreated);
        _userCreatedActivityFactory.Setup(x => x.Revision).Returns(1);

        listToFileConverters
            .Setup(x => x.GetEnumerator())
            .Returns(new[] { _csvMock.Object, _xlsxMock.Object }.ToList()
                .GetEnumerator());

        activityFactories
            .Setup(x => x.GetEnumerator())
            .Returns(new[] { _userCreatedActivityFactory.Object }.ToList().GetEnumerator());

        _dbContextMock = new TaxBeaconDbContext(
            new DbContextOptionsBuilder<TaxBeaconDbContext>()
                .UseInMemoryDatabase($"{nameof(UserServiceTests)}-InMemoryDb-{Guid.NewGuid()}")
                .Options,
            entitySaveChangesInterceptorMock.Object);

        _currentUserServiceMock.Setup(x => x.TenantId).Returns(_tenantId);

        createUserOptionsSnapshot.Setup(x => x.Value).Returns(new CreateUserOptions());

        _userService = new UserService(
            userServiceLoggerMock.Object,
            _dbContextMock,
            _dateTimeServiceMock.Object,
            _currentUserServiceMock.Object,
            listToFileConverters.Object,
            _userExternalStore.Object,
            dateTimeFormatterMock.Object,
            activityFactories.Object,
            createUserOptionsSnapshot.Object,
            emailSenderMock.Object);

        TypeAdapterConfig.GlobalSettings.Scan(typeof(IUserService).Assembly);
    }

    private static class TestData
    {
        public static readonly Guid TestTenantId = Guid.NewGuid();
        private static readonly Guid TestObjectId = Guid.NewGuid();
        public static readonly string TestEmail = "test@test.test";

        public static Faker<Tenant> TenantFaker =>
            new Faker<Tenant>()
                .RuleFor(t => t.Id, _ => TestTenantId)
                .RuleFor(t => t.Name, f => f.Company.CompanyName())
                .RuleFor(t => t.CreatedDateTimeUtc, _ => DateTime.UtcNow);

        public static Faker<User> UserFaker =>
            new Faker<User>()
                .RuleFor(u => u.Id, _ => Guid.NewGuid())
                .RuleFor(u => u.IdpExternalId, _ => TestObjectId.ToString())
                .RuleFor(u => u.FirstName, f => f.Name.FirstName())
                .RuleFor(u => u.LastName, f => f.Name.LastName())
                .RuleFor(u => u.LegalName, (_, u) => u.FirstName)
                .RuleFor(u => u.FullName, (_, u) => $"{u.FirstName} {u.LastName}")
                .RuleFor(u => u.Email, f => f.Internet.Email())
                .RuleFor(u => u.CreatedDateTimeUtc, _ => DateTime.UtcNow)
                .RuleFor(u => u.Status, f => f.PickRandom<Status>());

        public static Faker<User> UserWithFixedEmailFaker =>
            new Faker<User>()
                .RuleFor(u => u.Id, _ => Guid.NewGuid())
                .RuleFor(u => u.IdpExternalId, _ => TestObjectId.ToString())
                .RuleFor(u => u.FirstName, f => f.Name.FirstName())
                .RuleFor(u => u.LastName, f => f.Name.LastName())
                .RuleFor(u => u.LegalName, (_, u) => u.FirstName)
                .RuleFor(u => u.FullName, (_, u) => $"{u.FirstName} {u.LastName}")
                .RuleFor(u => u.Email, _ => TestEmail)
                .RuleFor(u => u.CreatedDateTimeUtc, _ => DateTime.UtcNow)
                .RuleFor(u => u.Status, f => f.PickRandom<Status>());

        public static Faker<UserView> UserViewFaker =>
            new Faker<UserView>()
                .RuleFor(u => u.Id, _ => Guid.NewGuid())
                .RuleFor(u => u.CreatedDateTimeUtc, _ => DateTime.UtcNow)
                .RuleFor(u => u.LastModifiedDateTimeUtc, _ => DateTime.UtcNow)
                .RuleFor(u => u.FirstName, f => f.Name.FirstName())
                .RuleFor(u => u.LegalName, (_, u) => u.FirstName)
                .RuleFor(u => u.LastName, f => f.Name.LastName())
                .RuleFor(u => u.Email, f => f.Internet.Email())
                .RuleFor(u => u.Status, f => f.PickRandom<Status>())
                .RuleFor(u => u.LastLoginDateTimeUtc, _ => DateTime.UtcNow)
                .RuleFor(u => u.FullName, (_, u) => $"{u.FirstName} {u.LastName}")
                .RuleFor(u => u.DeactivationDateTimeUtc, _ => DateTime.UtcNow)
                .RuleFor(u => u.ReactivationDateTimeUtc, _ => DateTime.UtcNow)
                .RuleFor(u => u.DivisionId, _ => Guid.NewGuid())
                .RuleFor(u => u.Division, f => f.Commerce.Department())
                .RuleFor(u => u.DepartmentId, _ => Guid.NewGuid())
                .RuleFor(u => u.Department, f => f.Commerce.Department())
                .RuleFor(u => u.ServiceAreaId, _ => Guid.NewGuid())
                .RuleFor(u => u.ServiceArea, f => f.Name.JobArea())
                .RuleFor(u => u.JobTitleId, _ => Guid.NewGuid())
                .RuleFor(u => u.JobTitle, f => f.Name.JobTitle())
                .RuleFor(u => u.TeamId, _ => Guid.NewGuid())
                .RuleFor(u => u.Team, f => f.Commerce.Department())
                .RuleFor(u => u.Roles, f => f.Name.JobTitle());

        public static Faker<TenantUserView> TenantUserViewFaker =>
            new Faker<TenantUserView>()
                .RuleFor(u => u.Id, _ => Guid.NewGuid())
                .RuleFor(u => u.CreatedDateTimeUtc, _ => DateTime.UtcNow)
                .RuleFor(u => u.LastModifiedDateTimeUtc, _ => DateTime.UtcNow)
                .RuleFor(u => u.FirstName, f => f.Name.FirstName())
                .RuleFor(u => u.LegalName, (_, u) => u.FirstName)
                .RuleFor(u => u.LastName, f => f.Name.LastName())
                .RuleFor(u => u.Email, f => f.Internet.Email())
                .RuleFor(u => u.Status, f => f.PickRandom<Status>())
                .RuleFor(u => u.LastLoginDateTimeUtc, _ => DateTime.UtcNow)
                .RuleFor(u => u.FullName, (_, u) => $"{u.FirstName} {u.LastName}")
                .RuleFor(u => u.DeactivationDateTimeUtc, _ => DateTime.UtcNow)
                .RuleFor(u => u.ReactivationDateTimeUtc, _ => DateTime.UtcNow)
                .RuleFor(u => u.DivisionId, _ => Guid.NewGuid())
                .RuleFor(u => u.Division, f => f.Commerce.Department())
                .RuleFor(u => u.DepartmentId, _ => Guid.NewGuid())
                .RuleFor(u => u.Department, f => f.Commerce.Department())
                .RuleFor(u => u.ServiceAreaId, _ => Guid.NewGuid())
                .RuleFor(u => u.ServiceArea, f => f.Name.JobArea())
                .RuleFor(u => u.JobTitleId, _ => Guid.NewGuid())
                .RuleFor(u => u.JobTitle, f => f.Name.JobTitle())
                .RuleFor(u => u.TeamId, _ => Guid.NewGuid())
                .RuleFor(u => u.Team, f => f.Commerce.Department())
                .RuleFor(u => u.Roles, f => f.Name.JobTitle())
                .RuleFor(u => u.UserIdPlusTenantId, _ => Guid.NewGuid().ToString() + TestTenantId);

        public static Faker<UpdateUserDto> UpdateUserDtoFaker =>
            new Faker<UpdateUserDto>()
                .RuleFor(dto => dto.FirstName, f => f.Name.FirstName())
                .RuleFor(dto => dto.LegalName, f => f.Name.FirstName())
                .RuleFor(dto => dto.LastName, f => f.Name.LastName())
                .RuleFor(dto => dto.DivisionId, _ => null)
                .RuleFor(dto => dto.DepartmentId, _ => null)
                .RuleFor(dto => dto.ServiceAreaId, _ => null)
                .RuleFor(dto => dto.TeamId, _ => null)
                .RuleFor(dto => dto.JobTitleId, _ => null);

        public static Faker<Role> RoleFaker =>
            new Faker<Role>()
                .RuleFor(u => u.Id, _ => Guid.NewGuid())
                .RuleFor(u => u.Name, f => f.Name.JobTitle());

        public static Faker<Division> DivisionFaker =>
            new Faker<Division>()
                .RuleFor(d => d.Id, _ => Guid.NewGuid())
                .RuleFor(d => d.Name, f => f.Company.CompanyName());

        public static Faker<Department> DepartmentFaker =>
            new Faker<Department>()
                .RuleFor(d => d.Id, _ => Guid.NewGuid())
                .RuleFor(d => d.Name, f => f.Commerce.Department());

        public static Faker<ServiceArea> ServiceAreaFaker =>
            new Faker<ServiceArea>()
                .RuleFor(sa => sa.Id, _ => Guid.NewGuid())
                .RuleFor(sa => sa.Name, f => f.Name.JobArea());

        public static Faker<JobTitle> JobTitleFaker =>
            new Faker<JobTitle>()
                .RuleFor(jt => jt.Id, _ => Guid.NewGuid())
                .RuleFor(jt => jt.Name, f => f.Name.JobTitle());

        public static Faker<Team> TeamFaker =>
            new Faker<Team>()
                .RuleFor(t => t.Id, _ => Guid.NewGuid())
                .RuleFor(t => t.Name, f => f.Company.CompanyName());

        public static readonly Faker<CreateUserDto> CreateUserDtoFaker = new Faker<CreateUserDto>()
            .RuleFor(dto => dto.FirstName, f => f.Name.FirstName())
            .RuleFor(dto => dto.LegalName, f => f.Name.FirstName())
            .RuleFor(dto => dto.LastName, f => f.Name.LastName())
            .RuleFor(dto => dto.Email, f => f.Internet.Email());

        public static async Task<(Guid divisionId, Guid departmentId, Guid serviceAreaId, Guid jobTitleId, Guid tenantId)>
            SeedOrganizationUnits(ITaxBeaconDbContext context)
        {
            var tenant = TenantFaker
                .RuleFor(t => t.Id, _ => Guid.NewGuid())
                .Generate();
            var division = DivisionFaker
                .RuleFor(d => d.TenantId, _ => tenant.Id)
                .Generate();
            var department = DepartmentFaker
                .RuleFor(d => d.DivisionId, division.Id)
                .RuleFor(d => d.TenantId, _ => tenant.Id)
                .Generate();
            var serviceArea = ServiceAreaFaker
                .RuleFor(sa => sa.DepartmentId, department.Id)
                .RuleFor(sa => sa.TenantId, _ => tenant.Id)
                .Generate();
            var jobTitle = JobTitleFaker
                .RuleFor(jt => jt.DepartmentId, department.Id)
                .RuleFor(jt => jt.TenantId, _ => tenant.Id)
                .Generate();

            await context.Tenants.AddAsync(tenant);
            await context.Divisions.AddAsync(division);
            await context.Departments.AddAsync(department);
            await context.ServiceAreas.AddAsync(serviceArea);
            await context.JobTitles.AddAsync(jobTitle);
            await context.SaveChangesAsync();

            return (division.Id, department.Id, serviceArea.Id, jobTitle.Id, tenant.Id);
        }
    }
}
