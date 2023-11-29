using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using TaxBeacon.Administration.Teams.Activities.Factories;
using TaxBeacon.Administration.Teams;
using TaxBeacon.Common.Converters;
using TaxBeacon.Common.Enums.Administration.Activities;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL.Administration.Entities;
using TaxBeacon.DAL.Administration;
using TaxBeacon.DAL.Interceptors;
using TaxBeacon.DAL;
using Bogus;
using TaxBeacon.Administration.Teams.Models;

namespace TaxBeacon.Administration.UnitTests.Services.Teams;
public partial class TeamServiceTests
{
    private readonly TeamService _teamService;
    private readonly ITaxBeaconDbContext _dbContextMock;
    private readonly Mock<IDateTimeService> _dateTimeServiceMock;
    private readonly Mock<EntitySaveChangesInterceptor> _entitySaveChangesInterceptorMock;
    private readonly Mock<IListToFileConverter> _csvMock;
    private readonly Mock<IListToFileConverter> _xlsxMock;
    private readonly Mock<ILogger<TeamService>> _teamServiceLoggerMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IDateTimeFormatter> _dateTimeFormatterMock;
    private readonly Mock<IEnumerable<IListToFileConverter>> _listToFileConverters;
    private readonly User _currentUser = TestData.TestUser.Generate();
    private readonly Mock<ITeamActivityFactory> _teamActivityFactory;
    private readonly Mock<IEnumerable<ITeamActivityFactory>> _activityFactories;
    public static readonly Guid TenantId = Guid.NewGuid();

    public TeamServiceTests()
    {
        _entitySaveChangesInterceptorMock = new();
        _dbContextMock = new TaxBeaconDbContext(
            new DbContextOptionsBuilder<TaxBeaconDbContext>()
                .UseInMemoryDatabase($"{nameof(TeamServiceTests)}-InMemoryDb-{Guid.NewGuid()}")
                .Options,
            _entitySaveChangesInterceptorMock.Object);

        _teamServiceLoggerMock = new();
        _dateTimeServiceMock = new();
        _csvMock = new();
        _xlsxMock = new();
        _dateTimeFormatterMock = new();
        _listToFileConverters = new();

        _csvMock.Setup(x => x.FileType).Returns(FileType.Csv);
        _xlsxMock.Setup(x => x.FileType).Returns(FileType.Xlsx);

        _listToFileConverters
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

        var currentUser = TestData.TestUser.Generate();
        _dbContextMock.Users.Add(currentUser);
        _dbContextMock.SaveChangesAsync().Wait();
        _currentUserServiceMock.Setup(x => x.UserId).Returns(currentUser.Id);

        _teamActivityFactory = new();
        _teamActivityFactory.Setup(x => x.EventType).Returns(TeamEventType.None);
        _teamActivityFactory.Setup(x => x.Revision).Returns(1);

        _activityFactories = new();
        _activityFactories
            .Setup(x => x.GetEnumerator())
            .Returns(new[] { _teamActivityFactory.Object }.ToList().GetEnumerator());

        _teamService = new TeamService(_currentUserServiceMock.Object, _teamServiceLoggerMock.Object, _dbContextMock, _dateTimeFormatterMock.Object,
            _dateTimeServiceMock.Object, _listToFileConverters.Object, _activityFactories.Object);

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
                .RuleFor(u => u.Department, f =>
                    new Department
                    {
                        Name = f.Name.FirstName()
                    })
                .RuleFor(u => u.JobTitle, f =>
                    new JobTitle
                    {
                        Name = f.Name.FirstName()
                    });

        public static readonly Faker<Team> TestTeam =
            new Faker<Team>()
                .RuleFor(u => u.Id, _ => Guid.NewGuid())
                .RuleFor(u => u.Name, f => f.Name.JobTitle())
                .RuleFor(u => u.CreatedDateTimeUtc, _ => DateTime.UtcNow)
                .RuleFor(u => u.Description, f => f.Lorem.Sentence(2))
                .RuleFor(u => u.TenantId, _ => TeamServiceTests.TenantId);

        public static readonly Faker<Tenant> TestTenant =
                new Faker<Tenant>()
                    .RuleFor(t => t.Id, _ => Guid.NewGuid())
                    .RuleFor(t => t.Name, f => f.Company.CompanyName())
                    .RuleFor(t => t.CreatedDateTimeUtc, _ => DateTime.UtcNow);

        public static readonly Faker<UpdateTeamDto> UpdateTeamDtoFaker =
           new Faker<UpdateTeamDto>()
               .RuleFor(t => t.Name, f => f.Name.JobType())
               .RuleFor(t => t.Description, f => f.Lorem.Sentence(2));
    }
}
