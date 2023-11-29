using FluentAssertions;

namespace TaxBeacon.Administration.UnitTests.Services.Tenants;
public partial class TenantServiceTests
{
    [Fact]
    public async Task GetAssignedTenantProgramsAsync_ExistingTenant_ReturnsListOfPrograms()
    {
        // Arrange
        var tenants = TestData.TenantFaker.Generate(2);
        tenants[0].TenantsPrograms.First().IsDeleted = true;
        await _dbContextMock.Tenants.AddRangeAsync(tenants);
        await _dbContextMock.SaveChangesAsync();

        // Act
        var actualResult = await _tenantService.GetTenantProgramsAsync(tenants[0].Id);

        // Assert
        actualResult.TryPickT0(out var actualPrograms, out _).Should().BeTrue();
        var actualProgramsIds = actualPrograms.Select(ap => ap.Id);
        actualProgramsIds.Should()
            .BeEquivalentTo(tenants[0].TenantsPrograms
                .Where(tp => tp.IsDeleted == false)
                .Select(tp => tp.ProgramId));
    }

    [Fact]
    public async Task GetAssignedTenantProgramsAsync_NotExistingTenantId_ReturnsNotFound()
    {
        // Act
        var actualResult = await _tenantService.GetTenantProgramsAsync(Guid.NewGuid());

        // Assert
        actualResult.IsT0.Should().BeFalse();
        actualResult.IsT1.Should().BeTrue();
    }
}
