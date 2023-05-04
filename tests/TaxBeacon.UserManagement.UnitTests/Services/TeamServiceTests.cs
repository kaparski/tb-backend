using Bogus;
using FluentAssertions;
using FluentAssertions.Execution;
using Gridify;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using TaxBeacon.Common.Converters;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL;
using TaxBeacon.DAL.Entities;
using TaxBeacon.DAL.Interceptors;
using TaxBeacon.DAL.Interfaces;
using TaxBeacon.UserManagement.Models;
using TaxBeacon.UserManagement.Models.Export;
using TaxBeacon.UserManagement.Services;
using TaxBeacon.UserManagement.Services.Activities;

namespace TaxBeacon.UserManagement.UnitTests.Services;

public class TeamServiceTests
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

    [Fact]
    public async Task GetTeamsAsync_AscendingOrderingAndPaginationOfLastPage_AscendingOrderOfTeamsAndCorrectPage()
    {
        // Arrange
        var teams = TestData.TestTeam.Generate(5);
        _currentUser.Team = teams[0];
        await _dbContextMock.Teams.AddRangeAsync(teams);
        await _dbContextMock.SaveChangesAsync();
        var query = new GridifyQuery
        {
            Page = 1,
            PageSize = 10,
            OrderBy = "name asc"
        };

        // Act
        var teamsOneOf = await _teamService.GetTeamsAsync(query, default);

        // Assert
        using (new AssertionScope())
        {
            teamsOneOf.TryPickT0(out var pageOfTeams, out _);
            pageOfTeams.Should().NotBeNull();
            var listOfTeams = pageOfTeams.Query.ToList();
            listOfTeams.Count.Should().Be(5);
            listOfTeams.Select(x => x.Name).Should().BeInAscendingOrder((o1, o2) => string.Compare(o1, o2, StringComparison.InvariantCultureIgnoreCase));
            pageOfTeams.Count.Should().Be(5);
        }
    }

    [Fact]
    public async Task GetTeamsAsync_DescendingOrderingAndPaginationWithFirstPage_CorrectNumberOfTeamsInDescendingOrder()
    {
        // Arrange
        var teams = TestData.TestTeam.Generate(7);
        await _dbContextMock.Teams.AddRangeAsync(teams);
        await _dbContextMock.SaveChangesAsync();
        var query = new GridifyQuery
        {
            Page = 1,
            PageSize = 4,
            OrderBy = "name desc"
        };

        // Act
        var teamsOneOf = await _teamService.GetTeamsAsync(query, default);

        // Assert
        using (new AssertionScope())
        {
            teamsOneOf.TryPickT0(out var pageOfTeams, out _);
            pageOfTeams.Should().NotBeNull();
            var listOfTeams = pageOfTeams.Query.ToList();
            listOfTeams.Count.Should().Be(4);
            listOfTeams.Select(x => x.Name).Should().BeInDescendingOrder();
            pageOfTeams.Count.Should().Be(7);
        }
    }

    [Fact]
    public async Task GetTeamsAsync_NoTeams_NumberOfTeamsEmpty()
    {
        // Arrange
        var query = new GridifyQuery
        {
            Page = 1,
            PageSize = 123,
            OrderBy = "name desc"
        };

        // Act
        var teamsOneOf = await _teamService.GetTeamsAsync(query, default);

        // Assert
        using (new AssertionScope())
        {
            teamsOneOf.TryPickT0(out var pageOfTeams, out _);
            pageOfTeams.Should().NotBeNull();
            var listOfTeams = pageOfTeams.Query.ToList();
            listOfTeams.Count.Should().Be(0);
            pageOfTeams.Count.Should().Be(0);
        }
    }

    [Theory]
    [InlineData(7, 25, 2)]
    [InlineData(10, 5, 3)]
    public async Task GetTeamsAsync_PageNumberOutsideOfTotalRange_TeamListIsEmpty(int numberOfTeams, int pageSize, int pageNumber)
    {
        // Arrange
        var teams = TestData.TestTeam.Generate(numberOfTeams);
        await _dbContextMock.Teams.AddRangeAsync(teams);
        await _dbContextMock.SaveChangesAsync();
        var query = new GridifyQuery
        {
            Page = pageNumber,
            PageSize = pageSize,
            OrderBy = "name asc",
        };

        // Act
        var teamsOneOf = await _teamService.GetTeamsAsync(query, default);

        // Assert
        teamsOneOf.TryPickT0(out var pageOfTeams, out _);
        pageOfTeams.Should().BeNull();
    }

    [Theory]
    [InlineData(FileType.Csv)]
    [InlineData(FileType.Xlsx)]
    public async Task ExportTeamsAsync_ValidInputData_AppropriateConverterShouldBeCalled(FileType fileType)
    {
        //Arrange
        var teams = TestData.TestTeam.Generate(5);

        await _dbContextMock.Teams.AddRangeAsync(teams);
        await _dbContextMock.SaveChangesAsync();

        //Act
        _ = await _teamService.ExportTeamsAsync(fileType, default);

        //Assert
        if (fileType == FileType.Csv)
        {
            _csvMock.Verify(x => x.Convert(It.IsAny<List<TeamExportModel>>()), Times.Once());
        }
        else if (fileType == FileType.Xlsx)
        {
            _xlsxMock.Verify(x => x.Convert(It.IsAny<List<TeamExportModel>>()), Times.Once());
        }
        else
        {
            throw new InvalidOperationException();
        }
    }

    [Fact]
    public async Task GetActivitiesAsync_TeamExists_ShouldCallAppropriateFactory()
    {
        //Arrange
        var team = TestData.TestTeam.Generate();
        team.TenantId = TenantId;

        var teamActivity = new TeamActivityLog()
        {
            Date = DateTime.UtcNow,
            TenantId = TenantId,
            Team = team,
            EventType = TeamEventType.None,
            Revision = 1
        };

        _dbContextMock.TeamActivityLogs.Add(teamActivity);
        await _dbContextMock.SaveChangesAsync();

        //Act
        await _teamService.GetActivitiesAsync(team.Id);

        //Assert

        _teamActivityFactory.Verify(x => x.Create(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task GetActivitiesAsync_DivisionDoesNotExistWithinCurrentTenant_ShouldReturnNotFound()
    {
        //Arrange
        var tenant = TestData.TestTenant.Generate();
        var team = TestData.TestTeam.Generate();
        team.Tenant = tenant;
        _dbContextMock.Tenants.Add(tenant);
        _dbContextMock.Teams.Add(team);
        await _dbContextMock.SaveChangesAsync();

        //Act
        var resultOneOf = await _teamService.GetActivitiesAsync(team.Id);

        //Assert
        resultOneOf.TryPickT1(out _, out _).Should().BeTrue();
    }

    [Fact]
    public async Task GetActivitiesAsync_TeamExists_ShouldReturnExpectedNumberOfItems()
    {
        //Arrange
        var team = TestData.TestTeam.Generate();
        team.TenantId = TenantId;

        var activities = new[]
        {
                new TeamActivityLog()
                {
                    Date = new DateTime(2000, 01, 1),
                    TenantId = TenantId,
                    Team = team,
                    EventType = TeamEventType.None,
                    Revision = 1
                },
            };

        _dbContextMock.TeamActivityLogs.AddRange(activities);
        await _dbContextMock.SaveChangesAsync();

        const int pageSize = 2;

        //Act
        var resultOneOf = await _teamService.GetActivitiesAsync(team.Id, 1, pageSize);

        //Assert
        using (new AssertionScope())
        {
            resultOneOf.TryPickT0(out var activitiesResult, out _).Should().BeTrue();
            activitiesResult.Count.Should().Be(1);
            activitiesResult.Query.Count().Should().Be(1);
        }
    }

    [Fact]
    public async Task GetDivisionDetailsAsync_ValidId_ReturnsDivision()
    {
        //Arrange
        TestData.TestTeam.RuleFor(
            x => x.TenantId, _ => TenantId);
        var teams = TestData.TestTeam.Generate(5);

        await _dbContextMock.Teams.AddRangeAsync(teams);
        await _dbContextMock.SaveChangesAsync();

        //Act
        var result = await _teamService.GetTeamDetailsAsync(teams[0].Id);

        //Assert
        using (new AssertionScope())
        {
            result.TryPickT0(out var teamDetails, out _).Should().BeTrue();
            teamDetails.Id.Should().Be(teams[0].Id);
        }
    }

    [Fact]
    public async Task GetTeamDetailsAsync_IdNotInDb_ReturnsNotFound()
    {
        //Arrange
        TestData.TestTeam.RuleFor(
            x => x.TenantId, _ => TenantId);
        var teams = TestData.TestTeam.Generate(5);

        await _dbContextMock.Teams.AddRangeAsync(teams);
        await _dbContextMock.SaveChangesAsync();

        //Act
        var result = await _teamService.GetTeamDetailsAsync(new Guid());

        //Assert
        result.TryPickT1(out _, out _).Should().BeTrue();
    }

    [Fact]
    public async Task UpdateTeamAsync_TeamExists_ReturnsUpdatedTeamAndCapturesActivityLog()
    {
        // Arrange
        var updateTeamDto = TestData.UpdateTeamDtoFaker.Generate();
        var team = TestData.TestTeam.Generate();
        await _dbContextMock.Teams.AddAsync(team);
        await _dbContextMock.SaveChangesAsync();

        var currentDate = DateTime.UtcNow;
        _dateTimeServiceMock
            .Setup(service => service.UtcNow)
            .Returns(currentDate);

        // Act
        var actualResult = await _teamService.UpdateTeamAsync(team.Id, updateTeamDto, default);

        // Assert
        using (new AssertionScope())
        {
            (await _dbContextMock.SaveChangesAsync()).Should().Be(0);
            actualResult.TryPickT0(out var divisionDto, out _);
            divisionDto.Should().NotBeNull();
            divisionDto.Id.Should().Be(team.Id);
            divisionDto.Name.Should().Be(updateTeamDto.Name);

            var actualActivityLog = await _dbContextMock.TeamActivityLogs.LastOrDefaultAsync();
            actualActivityLog.Should().NotBeNull();
            actualActivityLog?.Date.Should().Be(currentDate);
            actualActivityLog?.EventType.Should().Be(TeamEventType.TeamUpdatedEvent);
            actualActivityLog?.TenantId.Should().Be(TenantId);
            actualActivityLog?.TeamId.Should().Be(team.Id);

            _dateTimeServiceMock
                .Verify(ds => ds.UtcNow, Times.Once);
        }
    }

    [Fact]
    public async Task UpdateTeamAsync_TeamDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var updateTeamDto = TestData.UpdateTeamDtoFaker.Generate();
        var team = TestData.TestTeam.Generate();
        await _dbContextMock.Teams.AddAsync(team);
        await _dbContextMock.SaveChangesAsync();

        // Act
        var actualResult = await _teamService.UpdateTeamAsync(Guid.NewGuid(), updateTeamDto, default);

        // Assert
        using (new AssertionScope())
        {
            actualResult.TryPickT1(out var divisionDto, out _);
            divisionDto.Should().NotBeNull();
        }
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
                .RuleFor(u => u.Status, f => f.PickRandom<Status>());

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
