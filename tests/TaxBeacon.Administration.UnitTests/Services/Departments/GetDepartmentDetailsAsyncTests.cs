using FluentAssertions.Execution;
using FluentAssertions;

namespace TaxBeacon.Administration.UnitTests.Services.Departments;
public partial class DepartmentServiceTests
{
    [Fact]
    public async Task GetDepartmentDetailsAsync_NonExistentDepartment_ReturnsNotFound()
    {
        // Arrange
        var department = TestData.TestDepartment.Generate();
        await _dbContextMock.Departments.AddAsync(department);
        await _dbContextMock.SaveChangesAsync();

        // Act
        var actualResult = await _departmentService.GetDepartmentDetailsAsync(Guid.NewGuid());

        // Assert
        using (new AssertionScope())
        {
            actualResult.IsT0.Should().BeFalse();
            actualResult.IsT1.Should().BeTrue();
        }
    }

    [Fact]
    public async Task GetDepartmentDetailsAsync_DepartmentWithNoServiceAreas_ReturnsDepartmentDetails()
    {
        // Arrange
        var department = TestData.TestDepartment.Generate();

        var jobTitles = TestData.TestJobTitle.Generate(3);
        jobTitles.ForEach(department.JobTitles.Add);

        var division = TestData.TestDivision.Generate();
        department.Division = division;

        await _dbContextMock.Departments.AddAsync(department);
        await _dbContextMock.SaveChangesAsync();

        // Act
        var actualResult = await _departmentService.GetDepartmentDetailsAsync(department.Id);

        // Assert
        using (new AssertionScope())
        {
            actualResult.TryPickT0(out var departmentDetailsResult, out _).Should().BeTrue();

            departmentDetailsResult.Id.Should().Be(department.Id);
            departmentDetailsResult.Name.Should().Be(department.Name);
            departmentDetailsResult.Description.Should().Be(department.Description);
            departmentDetailsResult.Division.Id.Should().Be(division.Id);
            departmentDetailsResult.Division.Name.Should().Be(division.Name);
            departmentDetailsResult.ServiceAreas.Should().BeEmpty();
            departmentDetailsResult.JobTitles.Count.Should().Be(jobTitles.Count);
        }
    }

    [Fact]
    public async Task GetDepartmentDetailsAsync_ExistingDepartment_ReturnsDepartmentDetails()
    {
        // Arrange
        var department = TestData.TestDepartment.Generate();

        var serviceAreas = TestData.TestServiceArea.Generate(3);
        serviceAreas.ForEach(department.ServiceAreas.Add);

        var jobTitles = TestData.TestJobTitle.Generate(3);
        jobTitles.ForEach(department.JobTitles.Add);

        await _dbContextMock.Departments.AddAsync(department);
        await _dbContextMock.SaveChangesAsync();

        // Act
        var actualResult = await _departmentService.GetDepartmentDetailsAsync(department.Id);

        // Assert
        using (new AssertionScope())
        {
            actualResult.TryPickT0(out var departmentDetailsResult, out _).Should().BeTrue();

            departmentDetailsResult.Id.Should().Be(department.Id);
            departmentDetailsResult.Name.Should().Be(department.Name);
            departmentDetailsResult.Description.Should().Be(department.Description);
            departmentDetailsResult.Division.Should().BeNull();
            departmentDetailsResult.ServiceAreas.Count().Should().Be(serviceAreas.Count);
            departmentDetailsResult.JobTitles.Count().Should().Be(jobTitles.Count);
        }
    }
}
