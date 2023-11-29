using FluentAssertions.Execution;
using FluentAssertions;

namespace TaxBeacon.Administration.UnitTests.Services.JobTitles;
public partial class JobTitleServiceTests
{
    [Fact]
    public async Task QueryJobTitles_ReturnsJobTitles()
    {
        // Arrange
        var items = TestData.TestJobTitle.Generate(5);
        await _dbContextMock.JobTitles.AddRangeAsync(items);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock.Setup(x => x.TenantId).Returns(TestData.TestTenantId);

        // Act
        var query = _serviceAreaService.QueryJobTitles();
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
