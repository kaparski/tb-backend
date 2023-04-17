using Bogus;
using FluentAssertions;
using FluentAssertions.Execution;
using Gridify;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using TaxBeacon.Common.Converters;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Permissions;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL;
using TaxBeacon.DAL.Entities;
using TaxBeacon.DAL.Interceptors;
using TaxBeacon.DAL.Interfaces;
using TaxBeacon.UserManagement.Models.Export;
using TaxBeacon.UserManagement.Services;

namespace TaxBeacon.UserManagement.UnitTests.Services;

public class TeamServiceTests
{
    private readonly TeamService _teamService;
    private readonly ITaxBeaconDbContext _dbContextMock;
    private readonly Mock<IDateTimeService> _dateTimeServiceMock;
    private readonly Mock<EntitySaveChangesInterceptor> _entitySaveChangesInterceptorMock;
    private readonly Mock<IListToFileConverter> _csvMock;
    private readonly Mock<IListToFileConverter> _xlsxMock;
    private readonly Mock<ILogger<UserService>> _teamServiceLoggerMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IDateTimeFormatter> _dateTimeFormatterMock;
    private readonly Mock<IEnumerable<IListToFileConverter>> _listToFileConverters;
    private readonly User _currentUser = TestData.TestUser.Generate();
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
        _teamService = new TeamService(_currentUserServiceMock.Object, _teamServiceLoggerMock.Object, _dbContextMock, _dateTimeFormatterMock.Object,
            _dateTimeServiceMock.Object, _listToFileConverters.Object);

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
            listOfTeams.Select(x => x.Name).Should().BeInAscendingOrder();
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
    public async Task GetTeamsAsync_NoTenants_CorrectNumberOfTeams()
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

    [Fact]
    public async Task GetTeamsAsync_PageNumberOutsideOfTotalRange_TeamListIsEmpty()
    {
        // Arrange
        var teams = TestData.TestTeam.Generate(7);
        await _dbContextMock.Teams.AddRangeAsync(teams);
        await _dbContextMock.SaveChangesAsync();
        var query = new GridifyQuery
        {
            Page = 2,
            PageSize = 25,
            OrderBy = "name asc",
        };

        // Act
        var teamsOneOf = await _teamService.GetTeamsAsync(query, default);

        // Assert
        teamsOneOf.TryPickT0(out var pageOfTeams, out _);
        pageOfTeams.Should().BeNull();
    }

    [Fact]
    public async Task GetTeamsAsync_PageNumberRightOutsideOfTotalRange_TeamListIsEmpty()
    {
        // Arrange
        var teams = TestData.TestTeam.Generate(10);
        await _dbContextMock.Teams.AddRangeAsync(teams);
        await _dbContextMock.SaveChangesAsync();
        var query = new GridifyQuery
        {
            Page = 3,
            PageSize = 5,
            OrderBy = "name asc",
        };

        // Act
        var teamsOneOf = await _teamService.GetTeamsAsync(query, default);

        // Assert
        teamsOneOf.TryPickT0(out var pageOfTenants, out _);
        pageOfTenants.Should().BeNull();
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
    }
}
