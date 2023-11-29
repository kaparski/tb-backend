using FluentAssertions.Execution;
using FluentAssertions;

namespace TaxBeacon.Administration.UnitTests.Services.ServiceAreas;
public partial class ServiceAreaServiceTests
{
    [Fact]
    public async Task QueryServiceAreas_ReturnsJobTitles()
    {
        // Arrange
        var items = TestData.TestServiceArea.Generate(5);
        await _dbContextMock.ServiceAreas.AddRangeAsync(items);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock.Setup(x => x.TenantId).Returns(TestData.TestTenantId);

        // Act
        var query = _serviceAreaService.QueryServiceAreas();
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
