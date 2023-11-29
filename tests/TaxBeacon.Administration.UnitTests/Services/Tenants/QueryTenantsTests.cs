using FluentAssertions.Execution;
using FluentAssertions;

namespace TaxBeacon.Administration.UnitTests.Services.Tenants;
public partial class TenantServiceTests
{
    [Fact]
    public async Task QueryTenants_ReturnsTenants()
    {
        // Arrange
        var items = TestData.TenantFaker.Generate(5);
        await _dbContextMock.Tenants.AddRangeAsync(items);
        await _dbContextMock.SaveChangesAsync();

        // Act
        var query = _tenantService.QueryTenants();
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
            }
        }
    }
}
