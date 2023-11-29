using FluentAssertions.Execution;
using FluentAssertions;

namespace TaxBeacon.Administration.UnitTests.Services.Teams;
public partial class TeamServiceTests
{
    [Fact]
    public async Task QueryTeams_ReturnsJobTitles()
    {
        // Arrange
        var items = TestData.TestTeam.Generate(5);
        await _dbContextMock.Teams.AddRangeAsync(items);
        await _dbContextMock.SaveChangesAsync();

        // Act
        var query = _teamService.QueryTeams();
        var result = query.ToArray();

        // Assert

        using (new AssertionScope())
        {
            result.Should().HaveCount(5);

            foreach (var dto in result)
            {
                var item = items.Single(u => u.Id == dto.Id);

                dto.Should().BeEquivalentTo(item, opt => opt.ExcludingMissingMembers());
            }
        }
    }
}
