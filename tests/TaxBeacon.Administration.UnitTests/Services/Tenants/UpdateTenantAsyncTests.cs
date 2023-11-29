using FluentAssertions.Execution;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using TaxBeacon.Common.Enums.Administration.Activities;

namespace TaxBeacon.Administration.UnitTests.Services.Tenants;
public partial class TenantServiceTests
{
    [Fact]
    public async Task UpdateTenantAsync_TenantExists_ReturnsUpdatedTenantAndCapturesActivityLog()
    {
        // Arrange
        var updateTenantDto = TestData.UpdateTenantDtoFaker.Generate();
        var tenant = TestData.TenantFaker.Generate();
        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.SaveChangesAsync();

        var currentDate = DateTime.UtcNow;
        _dateTimeServiceMock
            .Setup(service => service.UtcNow)
            .Returns(currentDate);

        // Act
        var actualResult = await _tenantService.UpdateTenantAsync(tenant.Id, updateTenantDto);

        // Assert
        using (new AssertionScope())
        {
            (await _dbContextMock.SaveChangesAsync()).Should().Be(0);
            actualResult.TryPickT0(out var tenantDto, out _);
            tenantDto.Should().NotBeNull();
            tenantDto.Id.Should().Be(tenant.Id);
            tenantDto.Name.Should().Be(updateTenantDto.Name);

            var actualActivityLog = await _dbContextMock.TenantActivityLogs.LastOrDefaultAsync();
            actualActivityLog.Should().NotBeNull();
            actualActivityLog?.Date.Should().Be(currentDate);
            actualActivityLog?.EventType.Should().Be(TenantEventType.TenantUpdatedEvent);
            actualActivityLog?.TenantId.Should().Be(tenant.Id);

            _dateTimeServiceMock
                .Verify(ds => ds.UtcNow, Times.Once);
        }
    }

    [Fact]
    public async Task UpdateTenantAsync_TenantDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var updateTenantDto = TestData.UpdateTenantDtoFaker.Generate();
        var tenant = TestData.TenantFaker.Generate();
        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.SaveChangesAsync();

        // Act
        var actualResult = await _tenantService.UpdateTenantAsync(Guid.NewGuid(), updateTenantDto);

        // Assert
        using (new AssertionScope())
        {
            actualResult.TryPickT1(out var tenantDto, out _);
            tenantDto.Should().NotBeNull();
        }
    }
}
