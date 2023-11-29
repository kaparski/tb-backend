using FluentAssertions.Execution;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TaxBeacon.Common.Enums.Administration.Activities;
using Moq;

namespace TaxBeacon.Administration.UnitTests.Services.JobTitles;
public partial class JobTitleServiceTests
{
    [Fact]
    public async Task UpdateJobTitleDetailsAsync_JobTitleExistsAndTenantIdEqualsUserTenantId_ReturnsUpdatedJobTitleAndCapturesActivityLog()
    {
        // Arrange
        var updateJobTitleDto = TestData.UpdateJobTitleDtoFaker.Generate();
        var serviceArea = TestData.TestJobTitle.Generate();
        await _dbContextMock.JobTitles.AddAsync(serviceArea);
        await _dbContextMock.SaveChangesAsync();

        var currentDate = DateTime.UtcNow;
        _dateTimeServiceMock
            .Setup(service => service.UtcNow)
            .Returns(currentDate);

        _currentUserServiceMock.Setup(x => x.TenantId).Returns(serviceArea.TenantId);

        // Act
        var actualResult = await _serviceAreaService.UpdateJobTitleDetailsAsync(
            serviceArea.Id, updateJobTitleDto, default);

        // Assert
        using (new AssertionScope())
        {
            (await _dbContextMock.SaveChangesAsync()).Should().Be(0);
            actualResult.TryPickT0(out var serviceAreaDetailsDto, out _);
            serviceAreaDetailsDto.Should().NotBeNull();
            serviceAreaDetailsDto.Id.Should().Be(serviceArea.Id);
            serviceAreaDetailsDto.Name.Should().Be(updateJobTitleDto.Name);
            serviceAreaDetailsDto.Description.Should().Be(updateJobTitleDto.Description);

            var actualActivityLog = await _dbContextMock.JobTitleActivityLogs.LastOrDefaultAsync();
            actualActivityLog.Should().NotBeNull();
            actualActivityLog?.Date.Should().Be(currentDate);
            actualActivityLog?.EventType.Should().Be(JobTitleEventType.JobTitleUpdatedEvent);
            actualActivityLog?.JobTitleId.Should().Be(serviceArea.Id);
            actualActivityLog?.TenantId.Should().Be(serviceArea.TenantId);

            _dateTimeServiceMock
                .Verify(ds => ds.UtcNow, Times.Once);
        }
    }

    [Fact]
    public async Task UpdateJobTitleDetailsAsync_JobTitleExistsAndTenantIdDeosNotEqualUserTenantId_ReturnsNotFound()
    {
        // Arrange
        var updateJobTitleDto = TestData.UpdateJobTitleDtoFaker.Generate();
        var serviceArea = TestData.TestJobTitle.Generate();
        await _dbContextMock.JobTitles.AddAsync(serviceArea);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock.Setup(x => x.TenantId).Returns(Guid.NewGuid());

        // Act
        var actualResult = await _serviceAreaService.UpdateJobTitleDetailsAsync(
            Guid.NewGuid(), updateJobTitleDto, default);

        // Assert
        using (new AssertionScope())
        {
            actualResult.IsT1.Should().BeTrue();
            actualResult.IsT0.Should().BeFalse();
        }
    }

    [Fact]
    public async Task UpdateJobTitleDetailsAsync_JobTitleDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var updateJobTitleDto = TestData.UpdateJobTitleDtoFaker.Generate();
        var serviceArea = TestData.TestJobTitle.Generate();
        await _dbContextMock.JobTitles.AddAsync(serviceArea);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock.Setup(x => x.TenantId).Returns(serviceArea.TenantId);

        // Act
        var actualResult = await _serviceAreaService.UpdateJobTitleDetailsAsync(
            Guid.NewGuid(), updateJobTitleDto, default);

        // Assert
        using (new AssertionScope())
        {
            actualResult.IsT1.Should().BeTrue();
            actualResult.IsT0.Should().BeFalse();
        }
    }
}
