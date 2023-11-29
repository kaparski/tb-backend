using FluentAssertions.Execution;
using FluentAssertions;

namespace TaxBeacon.Administration.UnitTests.Services.Divisions;
public partial class DivisionsServiceTests
{
    [Fact]
    public async Task QueryDivisions_ReturnsDivisions()
    {
        // Arrange
        var items = TestData.TestDivision.Generate(5);
        await _dbContextMock.Divisions.AddRangeAsync(items);
        await _dbContextMock.SaveChangesAsync();

        // Act
        var query = _divisionsService.QueryDivisions();
        var result = query.ToArray();

        // Assert

        using (new AssertionScope())
        {
            result.Should().HaveCount(5);

            foreach (var dto in result)
            {
                var item = items.Single(u => u.Id == dto.Id);

                dto.Should().BeEquivalentTo(item, opt => opt
                    .ExcludingMissingMembers()
                    .Excluding(d => d.Departments)
                );
                dto.NumberOfDepartments.Should().Be(item.Departments.Count);
            }
        }
    }
}
