using FluentAssertions.Execution;
using FluentAssertions;
using TaxBeacon.Administration.Departments.Models;

namespace TaxBeacon.Administration.UnitTests.Services.Departments;
public partial class DepartmentServiceTests
{
    [Fact]
    public async Task GetDepartmentJobTitlesAsync_DepartmentExists_ShouldReturnDepartmentJobTitles()
    {
        //Arrange
        var department = TestData.TestDepartment.Generate();
        department.TenantId = TestData.TestTenantId;
        var jobTitles = TestData.TestJobTitle.Generate(5);
        department.JobTitles = jobTitles;
        await _dbContextMock.Departments.AddRangeAsync(department);
        await _dbContextMock.SaveChangesAsync();

        //Act
        var resultOneOf = await _departmentService.GetDepartmentJobTitlesAsync(department.Id);

        //Assert
        using (new AssertionScope())
        {
            resultOneOf.TryPickT0(out var jobTitleDtos, out _).Should().BeTrue();
            jobTitleDtos.Should().AllBeOfType<DepartmentJobTitleDto>();
            jobTitleDtos.Length.Should().Be(5);
        }
    }

    [Fact]
    public async Task GetDepartmentJobTitlesAsync_DepartmentDoesNotExist_ShouldReturnNotFound()
    {
        //Arrange
        var department = TestData.TestDepartment.Generate();
        department.TenantId = TestData.TestTenantId;
        var jobTitles = TestData.TestJobTitle.Generate(5);
        department.JobTitles = jobTitles;
        await _dbContextMock.Departments.AddRangeAsync(department);
        await _dbContextMock.SaveChangesAsync();

        //Act
        var resultOneOf = await _departmentService.GetDepartmentJobTitlesAsync(Guid.NewGuid());

        //Assert
        resultOneOf.IsT0.Should().BeFalse();
        resultOneOf.IsT1.Should().BeTrue();
    }

    [Fact]
    public async Task GetDepartmentJobTitlesAsync_UserIsFromDifferentTenant_ShouldReturnNotFound()
    {
        //Arrange
        var department = TestData.TestDepartment.Generate();
        department.TenantId = TestData.TestTenantId;
        var jobTitles = TestData.TestJobTitle.Generate(5);
        department.JobTitles = jobTitles;
        await _dbContextMock.Departments.AddRangeAsync(department);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock.Setup(x => x.TenantId).Returns(Guid.NewGuid());

        //Act
        var resultOneOf = await _departmentService.GetDepartmentJobTitlesAsync(department.Id);

        //Assert
        resultOneOf.IsT0.Should().BeFalse();
        resultOneOf.IsT1.Should().BeTrue();
    }
}
