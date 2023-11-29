using FluentAssertions.Execution;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace TaxBeacon.Administration.UnitTests.Services.Tenants;
public partial class TenantServiceTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ToggleDivisionsAsync_DivisionsState_ShouldToggleDivision(bool divisionEnabled)
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        tenant.DivisionEnabled = divisionEnabled;

        _dbContextMock.Tenants.Add(tenant);
        await _dbContextMock.SaveChangesAsync();

        // Act
        await _tenantService.ToggleDivisionsAsync(divisionEnabled);

        // Assert
        using (new AssertionScope())
        {
            var actualTenant = await _dbContextMock.Tenants.FirstAsync(x => x.Id == tenant.Id);
            actualTenant.DivisionEnabled.Should().Be(divisionEnabled);
        }
    }

    [Fact]
    public async Task ToggleDivisionsAsync_TenantDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        _currentUserServiceMock.Setup(x => x.TenantId).Returns(Guid.NewGuid());

        // Act
        var oneOfNotFound = await _tenantService.ToggleDivisionsAsync(false);

        // Assert
        oneOfNotFound.IsT1.Should().BeTrue();
    }
}
