using FluentAssertions.Execution;
using FluentAssertions;
using TaxBeacon.Common.Exceptions;

namespace TaxBeacon.Administration.UnitTests.Services.Divisions;
public partial class DivisionsServiceTests
{
    [Fact]
    public async Task QueryDivisionUsersAsync_DivisionExistsAndFilterApplied_ShouldReturnUsersWithSpecificDepartment()
    {
        //Arrange
        var division = TestData.TestDivision.Generate();
        division.TenantId = TenantId;
        var listOfUsers = TestData.TestUser.Generate(5);
        division.Users = listOfUsers;
        await _dbContextMock.Divisions.AddRangeAsync(division);
        await _dbContextMock.SaveChangesAsync();

        //Act
        var query = await _divisionsService.QueryDivisionUsersAsync(division.Id);

        var departmentName = listOfUsers.First().Department!.Name;

        var users = query
            .Where(u => u.Department == departmentName)
            .OrderBy(u => u.Department)
            .ToArray();

        //Assert
        using (new AssertionScope())
        {
            users.Length.Should().BeGreaterThan(0);
            users.Should().BeInAscendingOrder((o1, o2) => string.Compare(o1.Department, o2.Department, StringComparison.InvariantCultureIgnoreCase));
            users.Should().AllSatisfy(u => u.Department.Should().Be(users.First().Department));
        }
    }

    [Fact]
    public async Task QueryDivisionUsersAsync_DivisionDoesNotExist_ShouldThrow()
    {
        //Arrange
        var division = TestData.TestDivision.Generate();
        division.TenantId = TenantId;
        await _dbContextMock.Divisions.AddRangeAsync(division);
        await _dbContextMock.SaveChangesAsync();

        //Act
        var task = _divisionsService.QueryDivisionUsersAsync(new Guid());

        //Assert
        task.Exception!.InnerException.Should().BeOfType<NotFoundException>();
    }
}
