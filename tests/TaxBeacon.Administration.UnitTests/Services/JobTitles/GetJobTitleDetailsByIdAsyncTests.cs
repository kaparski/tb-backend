using FluentAssertions;

namespace TaxBeacon.Administration.UnitTests.Services.JobTitles;
public partial class JobTitleServiceTests
{
    [Fact]
    public async Task GetJobTitleDetailsByIdAsync_JobTitleTenantIdExistsAndIsEqualCurrentUserTenantId_ReturnsJobTitleDto()
    {
        // Arrange
        var serviceArea = TestData.TestJobTitle.Generate();

        await _dbContextMock.JobTitles.AddAsync(serviceArea);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock.Setup(x => x.TenantId).Returns(serviceArea.TenantId);

        // Act
        var actualResult = await _serviceAreaService.GetJobTitleDetailsByIdAsync(serviceArea.Id, default);

        // Assert
        actualResult.TryPickT0(out var serviceAreaDto, out _).Should().BeTrue();
        serviceAreaDto.Should().NotBeNull();
    }

    [Fact]
    public async Task GetJobTitleDetailsByIdAsync_NonExistingTenantId_ReturnsNotFound()
    {
        // Arrange
        var serviceArea = TestData.TestJobTitle.Generate();

        await _dbContextMock.JobTitles.AddAsync(serviceArea);
        await _dbContextMock.SaveChangesAsync();

        // Act
        var actualResult = await _serviceAreaService.GetJobTitleDetailsByIdAsync(Guid.NewGuid(), default);

        // Assert
        actualResult.IsT1.Should().BeTrue();
        actualResult.IsT0.Should().BeFalse();
    }

    [Fact]
    public async Task GetJobTitleDetailsByIdAsync_JobTitleTenantIdNotEqualCurrentUserTenantId_ReturnsNotFound()
    {
        // Arrange
        var serviceArea = TestData.TestJobTitle.Generate();

        await _dbContextMock.JobTitles.AddAsync(serviceArea);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock.Setup(x => x.TenantId).Returns(Guid.NewGuid());

        // Act
        var actualResult = await _serviceAreaService.GetJobTitleDetailsByIdAsync(serviceArea.Id, default);

        // Assert
        actualResult.IsT1.Should().BeTrue();
        actualResult.IsT0.Should().BeFalse();
    }
}
