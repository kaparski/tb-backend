using FluentAssertions.Execution;
using FluentAssertions;
using TaxBeacon.Administration.Departments.Models;

namespace TaxBeacon.Administration.UnitTests.Services.Departments;
public partial class DepartmentServiceTests
{
    [Fact]
    public async Task GetDepartmentServiceAreasAsync_DepartmentExists_ShouldReturnDepartmentServiceAreas()
    {
        //Arrange
        var department = TestData.TestDepartment.Generate();
        department.TenantId = TestData.TestTenantId;
        var serviceAreas = TestData.TestServiceArea.Generate(5);
        department.ServiceAreas = serviceAreas;
        await _dbContextMock.Departments.AddRangeAsync(department);
        await _dbContextMock.SaveChangesAsync();

        //Act
        var resultOneOf = await _departmentService.GetDepartmentServiceAreasAsync(department.Id);

        //Assert
        using (new AssertionScope())
        {
            resultOneOf.TryPickT0(out var serviceAreaDtos, out _).Should().BeTrue();
            serviceAreaDtos.Should().AllBeOfType<DepartmentServiceAreaDto>();
            serviceAreaDtos.Length.Should().Be(5);
        }
    }

    [Fact]
    public async Task GetDepartmentServiceAreasAsync_DepartmentDoesNotExist_ShouldReturnNotFound()
    {
        //Arrange
        var department = TestData.TestDepartment.Generate();
        department.TenantId = TestData.TestTenantId;
        var serviceAreas = TestData.TestServiceArea.Generate(5);
        department.ServiceAreas = serviceAreas;
        await _dbContextMock.Departments.AddRangeAsync(department);
        await _dbContextMock.SaveChangesAsync();

        //Act
        var resultOneOf = await _departmentService.GetDepartmentServiceAreasAsync(Guid.NewGuid());

        //Assert
        resultOneOf.IsT0.Should().BeFalse();
        resultOneOf.IsT1.Should().BeTrue();
    }

    [Fact]
    public async Task GetDepartmentServiceAreasAsync_UserIsFromDifferentTenant_ShouldReturnNotFound()
    {
        //Arrange
        var department = TestData.TestDepartment.Generate();
        department.TenantId = TestData.TestTenantId;
        var serviceAreas = TestData.TestServiceArea.Generate(5);
        department.ServiceAreas = serviceAreas;
        await _dbContextMock.Departments.AddRangeAsync(department);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock.Setup(x => x.TenantId).Returns(Guid.NewGuid());

        //Act
        var resultOneOf = await _departmentService.GetDepartmentServiceAreasAsync(department.Id);

        //Assert
        resultOneOf.IsT0.Should().BeFalse();
        resultOneOf.IsT1.Should().BeTrue();
    }
}
