using FluentAssertions.Execution;
using FluentAssertions;

namespace TaxBeacon.Administration.UnitTests.Services.Departments;
public partial class DepartmentServiceTests
{
    [Fact]
    public async Task QueryDepartments_ReturnsDepartments()
    {
        // Arrange
        var items = TestData.TestDepartment.Generate(5);
        await _dbContextMock.Departments.AddRangeAsync(items);
        await _dbContextMock.SaveChangesAsync();

        // Act
        var query = _departmentService.QueryDepartments();
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
