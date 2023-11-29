using FluentAssertions.Execution;
using FluentAssertions;
using Moq;
using TaxBeacon.Common.Enums.Administration.Activities;
using TaxBeacon.DAL.Administration.Entities;

namespace TaxBeacon.Administration.UnitTests.Services.Teams;
public partial class TeamServiceTests
{
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
    public async Task GetActivitiesAsync_TeamDoesNotExistWithinCurrentTenant_ShouldReturnNotFound()
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
}
