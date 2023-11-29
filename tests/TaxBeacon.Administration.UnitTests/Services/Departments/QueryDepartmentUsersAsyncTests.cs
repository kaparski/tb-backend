using FluentAssertions.Execution;
using FluentAssertions;
using TaxBeacon.DAL.Administration.Entities;
using TaxBeacon.Common.Exceptions;

namespace TaxBeacon.Administration.UnitTests.Services.Departments;
public partial class DepartmentServiceTests
{
    [Fact]
    public async Task QueryDepartmentUsersAsync_DepartmentExistsAndFilterApplied_ShouldReturnUsersWithSpecificDepartment()
    {
        //Arrange
        var department = TestData.TestDepartment.Generate();
        department.TenantId = TestData.TestTenantId;
        var listOfUsers = TestData.TestUser.Generate(5);
        department.Users = listOfUsers;
        await _dbContextMock.TenantUsers.AddRangeAsync(department.Users.Select(u => new TenantUser { UserId = u.Id, TenantId = TestData.TestTenantId }));
        await _dbContextMock.Departments.AddRangeAsync(department);
        await _dbContextMock.SaveChangesAsync();

        //Act
        var query = await _departmentService.QueryDepartmentUsersAsync(department.Id);

        var teamName = listOfUsers.First().Team!.Name;
        var users = query
            .Where(u => u.Team == teamName)
            .OrderBy(u => u.Team)
            .ToArray();

        //Assert
        using (new AssertionScope())
        {
            users.Length.Should().BeGreaterThan(0);
            users.Should().BeInAscendingOrder((o1, o2) => string.Compare(o1.Team, o2.Team, StringComparison.InvariantCultureIgnoreCase));
            users.Should().AllSatisfy(u => u.Team.Should().Be(users.First().Team));
        }
    }

    [Fact]
    public async Task QueryDepartmentUsersAsync_DepartmentDoesNotExist_ShouldThrow()
    {
        //Arrange
        var department = TestData.TestDepartment.Generate();
        department.TenantId = TestData.TestTenantId;
        await _dbContextMock.Departments.AddRangeAsync(department);
        await _dbContextMock.SaveChangesAsync();

        //Act
        var task = _departmentService.QueryDepartmentUsersAsync(new Guid());

        //Assert
        task.Exception!.InnerException.Should().BeOfType<NotFoundException>();
    }

    [Fact]
    public async Task QueryDepartmentUsersAsync_UserIsFromDifferentTenant_ShouldThrow()
    {
        //Arrange
        var department = TestData.TestDepartment.Generate();
        department.TenantId = TestData.TestTenantId;
        await _dbContextMock.Departments.AddRangeAsync(department);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock.Setup(x => x.TenantId).Returns(Guid.NewGuid());

        //Act
        var task = _departmentService.QueryDepartmentUsersAsync(department.Id);

        //Assert
        task.Exception!.InnerException.Should().BeOfType<NotFoundException>();
    }
}
