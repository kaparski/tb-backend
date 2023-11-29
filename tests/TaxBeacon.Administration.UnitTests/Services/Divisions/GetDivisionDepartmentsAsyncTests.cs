using FluentAssertions.Execution;
using FluentAssertions;
using TaxBeacon.Administration.Divisions.Models;

namespace TaxBeacon.Administration.UnitTests.Services.Divisions;
public partial class DivisionsServiceTests
{
    [Fact]
    public async Task GetDivisionDepartmentsAsync_DivisionExists_ShouldReturnDivisionDepartments()
    {
        //Arrange
        var division = TestData.TestDivision.Generate();
        division.TenantId = TenantId;
        var departments = TestData.TestDepartment.Generate(5);
        division.Departments = departments;
        await _dbContextMock.Divisions.AddRangeAsync(division);
        await _dbContextMock.SaveChangesAsync();

        //Act
        var resultOneOf = await _divisionsService.GetDivisionDepartmentsAsync(division.Id);

        //Assert
        using (new AssertionScope())
        {
            resultOneOf.TryPickT0(out var divisionDepartmentDtos, out _).Should().BeTrue();
            divisionDepartmentDtos.Should().AllBeOfType<DivisionDepartmentDto>();
            divisionDepartmentDtos.Length.Should().Be(5);
        }
    }

    [Fact]
    public async Task GetDivisionDepartmentsAsync_DivisionDoesNotExist_ShouldReturnNotFound()
    {
        //Arrange
        var division = TestData.TestDivision.Generate();
        division.TenantId = TenantId;
        var departments = TestData.TestDepartment.Generate(5);
        division.Departments = departments;
        await _dbContextMock.Divisions.AddRangeAsync(division);
        await _dbContextMock.SaveChangesAsync();

        //Act
        var resultOneOf = await _divisionsService.GetDivisionDepartmentsAsync(Guid.NewGuid());

        //Assert
        resultOneOf.IsT0.Should().BeFalse();
        resultOneOf.IsT1.Should().BeTrue();
    }

    [Fact]
    public async Task GetDivisionDepartmentsAsync_UserIsFromDifferentTenant_ShouldReturnNotFound()
    {
        //Arrange
        var division = TestData.TestDivision.Generate();
        division.TenantId = TenantId;
        var departments = TestData.TestDepartment.Generate(5);
        division.Departments = departments;
        await _dbContextMock.Divisions.AddRangeAsync(division);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock.Setup(x => x.TenantId).Returns(Guid.NewGuid());

        //Act
        var resultOneOf = await _divisionsService.GetDivisionDepartmentsAsync(division.Id);

        //Assert
        resultOneOf.IsT0.Should().BeFalse();
        resultOneOf.IsT1.Should().BeTrue();
    }
}
