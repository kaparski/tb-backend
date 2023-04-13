using Bogus;
using Microsoft.Extensions.Logging;
using Moq;
using TaxBeacon.Common.Converters;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL.Entities;
using TaxBeacon.DAL.Interfaces;
using TaxBeacon.UserManagement.Services;

namespace TaxBeacon.UserManagement.UnitTests.Services;

public class TeamServiceTests
{
    private readonly TeamService _teamService;
    private readonly ITaxBeaconDbContext _dbContextMock;
    private readonly Mock<IDateTimeService> _dateTimeServiceMock;
    private readonly Mock<IListToFileConverter> _csvMock;
    private readonly Mock<IListToFileConverter> _xlsxMock;
    private readonly Mock<ILogger<UserService>> _teamServiceLoggerMock;
    private readonly Mock<ICurrentUserService> _currentTeamServiceMock;
    private readonly Mock<IDateTimeFormatter> _dateTimeFormatterMock;
    private readonly Mock<IEnumerable<IListToFileConverter>> _listToFileConverters;

    public TeamServiceTests()
    {
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
            .Returns(new[] { _csvMock.Object, _xlsxMock.Object }.ToList()
                .GetEnumerator());
        _currentTeamServiceMock = new();

        _teamService = new TeamService(_currentTeamServiceMock.Object, _teamServiceLoggerMock.Object, _dbContextMock,_dateTimeFormatterMock.Object,_dateTimeServiceMock.Object, _listToFileConverters.Object);
    }

    private static class TestData
    {
        public static readonly Faker<Tenant> TestTenant =
            new Faker<Tenant>()
                .RuleFor(t => t.Id, _ => Guid.NewGuid())
                .RuleFor(t => t.Name, f => f.Company.CompanyName())
                .RuleFor(t => t.CreatedDateTimeUtc, _ => DateTime.UtcNow);

        public static readonly Faker<User> TestUser =
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
