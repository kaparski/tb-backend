using FluentAssertions.Execution;
using FluentAssertions;

namespace TaxBeacon.Administration.UnitTests.Services.JobTitles;
public partial class JobTitleServiceTests
{
    [Fact]
    public async Task GetActivityHistory_JobTitleExistsAndTenantIdDoesNotEqualUserTenantId_ReturnsNotFound()
    {
        // Arrange
        var serviceArea = TestData.TestJobTitle.Generate();

        await _dbContextMock.JobTitles.AddAsync(serviceArea);
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
    public async Task GetActivityHistory_JobTitleExists_ReturnsNotFound()
    {
        // Arrange
        var serviceArea = TestData.TestJobTitle.Generate();

        await _dbContextMock.JobTitles.AddAsync(serviceArea);
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
