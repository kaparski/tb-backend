using FluentAssertions.Execution;
using FluentAssertions;

namespace TaxBeacon.Administration.UnitTests.Services.Programs;
public partial class ProgramServiceTests
{
    [Fact]
    public async Task QueryTenantPrograms_QueryIsValidOrderByNameDescending_ReturnsListOfPrograms()
    {
        // Arrange
        var programs = TestData.TenantProgramFaker.Generate(10);
        await _dbContextMock.TenantsPrograms.AddRangeAsync(programs);
        await _dbContextMock.SaveChangesAsync();

        // Act
        var query = _programService.QueryTenantPrograms();
        var tenantProgramDtos = query
            .OrderByDescending(p => p.Name)
            .Take(5)
            .ToArray();

        // Arrange
        using (new AssertionScope())
        {
            tenantProgramDtos.Length.Should().Be(5);
            tenantProgramDtos.Select(x => x.Name).Should().BeInDescendingOrder();
        }
    }
}
