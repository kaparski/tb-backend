using FluentAssertions.Execution;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TaxBeacon.Common.Enums.Administration.Activities;
using Moq;

namespace TaxBeacon.Administration.UnitTests.Services.Teams;
public partial class TeamServiceTests
{
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
            actualResult.TryPickT0(out var teamDto, out _);
            teamDto.Should().NotBeNull();
            teamDto.Id.Should().Be(team.Id);
            teamDto.Name.Should().Be(updateTeamDto.Name);

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
            actualResult.TryPickT1(out var teamDto, out _);
            teamDto.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task UpdateTeamAsync_UserTenantIdNotEqualTeamTenantId_ReturnsNotFound()
    {
        // Arrange
        var updateTeamDto = TestData.UpdateTeamDtoFaker.Generate();
        var team = TestData.TestTeam.Generate();
        team.TenantId = new Guid();
        await _dbContextMock.Teams.AddAsync(team);
        await _dbContextMock.SaveChangesAsync();

        // Act
        var actualResult = await _teamService.UpdateTeamAsync(Guid.NewGuid(), updateTeamDto, default);

        // Assert
        using (new AssertionScope())
        {
            actualResult.TryPickT1(out _, out _).Should().BeTrue();
        }
    }
}
