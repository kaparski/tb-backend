using FluentAssertions;

namespace TaxBeacon.Administration.UnitTests.Services.ServiceAreas;
public partial class ServiceAreaServiceTests
{
    [Fact]
    public async Task GetServiceAreaDetailsByIdAsync_ServiceAreaTenantIdExistsAndIsEqualCurrentUserTenantId_ReturnsServiceAreaDto()
    {
        // Arrange
        var serviceArea = TestData.TestServiceArea.Generate();

        await _dbContextMock.ServiceAreas.AddAsync(serviceArea);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock.Setup(x => x.TenantId).Returns(serviceArea.TenantId);

        // Act
        var actualResult = await _serviceAreaService.GetServiceAreaDetailsByIdAsync(serviceArea.Id, default);

        // Assert
        actualResult.TryPickT0(out var serviceAreaDto, out _).Should().BeTrue();
        serviceAreaDto.Should().NotBeNull();
    }

    [Fact]
    public async Task GetServiceAreaDetailsByIdAsync_NonExistingTenantId_ReturnsNotFound()
    {
        // Arrange
        var serviceArea = TestData.TestServiceArea.Generate();

        await _dbContextMock.ServiceAreas.AddAsync(serviceArea);
        await _dbContextMock.SaveChangesAsync();

        // Act
        var actualResult = await _serviceAreaService.GetServiceAreaDetailsByIdAsync(Guid.NewGuid(), default);

        // Assert
        actualResult.IsT1.Should().BeTrue();
        actualResult.IsT0.Should().BeFalse();
    }

    [Fact]
    public async Task GetServiceAreaDetailsByIdAsync_ServiceAreaTenantIdNotEqualCurrentUserTenantId_ReturnsNotFound()
    {
        // Arrange
        var serviceArea = TestData.TestServiceArea.Generate();

        await _dbContextMock.ServiceAreas.AddAsync(serviceArea);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock.Setup(x => x.TenantId).Returns(Guid.NewGuid());

        // Act
        var actualResult = await _serviceAreaService.GetServiceAreaDetailsByIdAsync(serviceArea.Id, default);

        // Assert
        actualResult.IsT1.Should().BeTrue();
        actualResult.IsT0.Should().BeFalse();
    }
}
