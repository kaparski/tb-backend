using FluentAssertions.Execution;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TaxBeacon.Common.Enums.Administration.Activities;
using Moq;

namespace TaxBeacon.Administration.UnitTests.Services.Departments;
public partial class DepartmentServiceTests
{
    [Fact]
    public async Task UpdateDepartmentAsync_DepartmentExists_ReturnsUpdatedDepartmentAndCapturesActivityLog()
    {
        // Arrange
        var updateDepartmentDto = TestData.UpdateDepartmentDtoFaker.Generate();
        var department = TestData.TestDepartment.Generate();
        await _dbContextMock.Departments.AddAsync(department);
        await _dbContextMock.SaveChangesAsync();

        var currentDate = DateTime.UtcNow;
        _dateTimeServiceMock
            .Setup(service => service.UtcNow)
            .Returns(currentDate);

        // Act
        var actualResult = await _departmentService.UpdateDepartmentAsync(department.Id, updateDepartmentDto, default);

        // Assert
        using (new AssertionScope())
        {
            (await _dbContextMock.SaveChangesAsync()).Should().Be(0);
            actualResult.TryPickT0(out var departmentDto, out _);
            departmentDto.Should().NotBeNull();
            departmentDto.Id.Should().Be(department.Id);
            departmentDto.Name.Should().Be(updateDepartmentDto.Name);

            var actualActivityLog = await _dbContextMock.DepartmentActivityLogs.LastOrDefaultAsync();
            actualActivityLog.Should().NotBeNull();
            actualActivityLog?.Date.Should().Be(currentDate);
            actualActivityLog?.EventType.Should().Be(DepartmentEventType.DepartmentUpdatedEvent);
            actualActivityLog?.DepartmentId.Should().Be(department.Id);

            _dateTimeServiceMock
                .Verify(ds => ds.UtcNow, Times.Once);
        }
    }

    [Fact]
    public async Task UpdateDepartmentAsync_DepartmentDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var updateDepartmentDto = TestData.UpdateDepartmentDtoFaker.Generate();
        var department = TestData.TestDepartment.Generate();

        // Act
        var actualResult = await _departmentService.UpdateDepartmentAsync(department.Id, updateDepartmentDto, default);

        // Assert
        using (new AssertionScope())
        {
            actualResult.IsT0.Should().BeFalse();
            actualResult.IsT1.Should().BeTrue();
        }
    }
}
