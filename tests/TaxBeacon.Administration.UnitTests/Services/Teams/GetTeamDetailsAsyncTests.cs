using FluentAssertions.Execution;
using FluentAssertions;

namespace TaxBeacon.Administration.UnitTests.Services.Teams;
public partial class TeamServiceTests
{
    [Fact]
    public async Task GetTeamDetailsAsync_ValidId_ReturnsTeam()
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
    public async Task GetTeamDetailsAsync_UserTenantIdNotEqualTeamTenantId_ReturnsNotFound()
    {
        //Arrange
        var teams = TestData.TestTeam.Generate(5);
        teams.ForEach(t => t.TenantId = new Guid());

        await _dbContextMock.Teams.AddRangeAsync(teams);
        await _dbContextMock.SaveChangesAsync();

        //Act
        var result = await _teamService.GetTeamDetailsAsync(teams[0].Id);

        //Assert
        result.TryPickT1(out _, out _).Should().BeTrue();
    }
}
