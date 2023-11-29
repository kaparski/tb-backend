using FluentAssertions.Execution;
using FluentAssertions;

namespace TaxBeacon.Administration.UnitTests.Services.Programs;
public partial class ProgramServiceTests
{
    [Fact]
    public async Task QueryPrograms_QueryIsValidOrderByNameDescending_ReturnsListOfPrograms()
    {
        // Arrange
        var programs = TestData.ProgramFaker.Generate(10);
        await _dbContextMock.Programs.AddRangeAsync(programs);
        await _dbContextMock.SaveChangesAsync();

        // Act
        var query = _programService.QueryPrograms();
        var listOfPrograms = query
            .OrderByDescending(p => p.Name)
            .Take(5)
            .ToArray();

        // Arrange
        using (new AssertionScope())
        {
            listOfPrograms.Length.Should().Be(5);
            listOfPrograms.Select(x => x.Name).Should().BeInDescendingOrder();
        }
    }
}
