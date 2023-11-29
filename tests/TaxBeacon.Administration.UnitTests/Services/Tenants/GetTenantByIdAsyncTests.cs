using FluentAssertions;

namespace TaxBeacon.Administration.UnitTests.Services.Tenants;
public partial class TenantServiceTests
{
    [Fact]
    public async Task GetTenantByIdAsync_ExistingTenantId_ReturnsTenantDto()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.SaveChangesAsync();

        // Act
        var actualResult = await _tenantService.GetTenantByIdAsync(tenant.Id);

        // Assert
        actualResult.TryPickT0(out var tenantDto, out _);
        tenantDto.Should().NotBeNull();
    }

    [Fact]
    public async Task GetTenantByIdAsync_NonExistingTenantId_ReturnsNotFound()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.SaveChangesAsync();

        // Act
        var actualResult = await _tenantService.GetTenantByIdAsync(Guid.NewGuid());

        // Assert
        actualResult.TryPickT1(out var _, out var _).Should().BeTrue();
    }
}
