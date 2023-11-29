using FluentAssertions.Execution;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TaxBeacon.Common.Enums.Administration.Activities;
using Moq;

namespace TaxBeacon.Administration.UnitTests.Services.ServiceAreas;
public partial class ServiceAreaServiceTests
{
    [Fact]
    public async Task UpdateServiceAreaDetailsAsync_ServiceAreaExistsAndTenantIdEqualsUserTenantId_ReturnsUpdatedServiceAreaAndCapturesActivityLog()
    {
        // Arrange
        var updateServiceAreaDto = TestData.UpdateServiceAreaDtoFaker.Generate();
        var serviceArea = TestData.TestServiceArea.Generate();
        await _dbContextMock.ServiceAreas.AddAsync(serviceArea);
        await _dbContextMock.SaveChangesAsync();

        var currentDate = DateTime.UtcNow;
        _dateTimeServiceMock
            .Setup(service => service.UtcNow)
            .Returns(currentDate);

        _currentUserServiceMock.Setup(x => x.TenantId).Returns(serviceArea.TenantId);

        // Act
        var actualResult = await _serviceAreaService.UpdateServiceAreaDetailsAsync(
            serviceArea.Id, updateServiceAreaDto, default);

        // Assert
        using (new AssertionScope())
        {
            (await _dbContextMock.SaveChangesAsync()).Should().Be(0);
            actualResult.TryPickT0(out var serviceAreaDetailsDto, out _);
            serviceAreaDetailsDto.Should().NotBeNull();
            serviceAreaDetailsDto.Id.Should().Be(serviceArea.Id);
            serviceAreaDetailsDto.Name.Should().Be(updateServiceAreaDto.Name);
            serviceAreaDetailsDto.Description.Should().Be(updateServiceAreaDto.Description);

            var actualActivityLog = await _dbContextMock.ServiceAreaActivityLogs.LastOrDefaultAsync();
            actualActivityLog.Should().NotBeNull();
            actualActivityLog?.Date.Should().Be(currentDate);
            actualActivityLog?.EventType.Should().Be(ServiceAreaEventType.ServiceAreaUpdatedEvent);
            actualActivityLog?.ServiceAreaId.Should().Be(serviceArea.Id);
            actualActivityLog?.TenantId.Should().Be(serviceArea.TenantId);

            _dateTimeServiceMock
                .Verify(ds => ds.UtcNow, Times.Once);
        }
    }

    [Fact]
    public async Task UpdateServiceAreaDetailsAsync_ServiceAreaExistsAndTenantIdDeosNotEqualUserTenantId_ReturnsNotFound()
    {
        // Arrange
        var updateServiceAreaDto = TestData.UpdateServiceAreaDtoFaker.Generate();
        var serviceArea = TestData.TestServiceArea.Generate();
        await _dbContextMock.ServiceAreas.AddAsync(serviceArea);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock.Setup(x => x.TenantId).Returns(Guid.NewGuid());

        // Act
        var actualResult = await _serviceAreaService.UpdateServiceAreaDetailsAsync(
            Guid.NewGuid(), updateServiceAreaDto, default);

        // Assert
        using (new AssertionScope())
        {
            actualResult.IsT1.Should().BeTrue();
            actualResult.IsT0.Should().BeFalse();
        }
    }

    [Fact]
    public async Task UpdateServiceAreaDetailsAsync_ServiceAreaDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var updateServiceAreaDto = TestData.UpdateServiceAreaDtoFaker.Generate();
        var serviceArea = TestData.TestServiceArea.Generate();
        await _dbContextMock.ServiceAreas.AddAsync(serviceArea);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock.Setup(x => x.TenantId).Returns(serviceArea.TenantId);

        // Act
        var actualResult = await _serviceAreaService.UpdateServiceAreaDetailsAsync(
            Guid.NewGuid(), updateServiceAreaDto, default);

        // Assert
        using (new AssertionScope())
        {
            actualResult.IsT1.Should().BeTrue();
            actualResult.IsT0.Should().BeFalse();
        }
    }
}
