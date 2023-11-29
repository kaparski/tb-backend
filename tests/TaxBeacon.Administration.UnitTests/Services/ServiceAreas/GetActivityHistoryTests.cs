using FluentAssertions.Execution;
using FluentAssertions;

namespace TaxBeacon.Administration.UnitTests.Services.ServiceAreas;
public partial class ServiceAreaServiceTests
{
    [Fact]
    public async Task GetActivityHistory_ServiceAreaExistsAndTenantIdDoesNotEqualUserTenantId_ReturnsNotFound()
    {
        // Arrange
        var serviceArea = TestData.TestServiceArea.Generate();

        await _dbContextMock.ServiceAreas.AddAsync(serviceArea);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock.Setup(x => x.TenantId).Returns(Guid.NewGuid());

        // Act
        var actualResult = await _serviceAreaService.GetActivityHistoryAsync(serviceArea.Id);

        //Assert
        using (new AssertionScope())
        {
            actualResult.IsT1.Should().BeTrue();
            actualResult.IsT0.Should().BeFalse();
        }
    }

    [Fact]
    public async Task GetActivityHistory_ServiceAreaExists_ReturnsNotFound()
    {
        // Arrange
        var serviceArea = TestData.TestServiceArea.Generate();

        await _dbContextMock.ServiceAreas.AddAsync(serviceArea);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock.Setup(x => x.TenantId).Returns(serviceArea.TenantId);

        // Act
        var actualResult = await _serviceAreaService.GetActivityHistoryAsync(Guid.NewGuid());

        //Assert
        using (new AssertionScope())
        {
            actualResult.IsT1.Should().BeTrue();
            actualResult.IsT0.Should().BeFalse();
        }
    }
}
