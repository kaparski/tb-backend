using FluentAssertions.Execution;
using FluentAssertions;
using TaxBeacon.DAL.Administration.Entities;
using TaxBeacon.Common.Exceptions;

namespace TaxBeacon.Administration.UnitTests.Services.Teams;
public partial class TeamServiceTests
{
    [Fact]
    public async Task QueryTeamUsersAsync_TeamExistsAndFilterApplied_ShouldReturnUsersWithSpecificTeam()
    {
        //Arrange
        var team = TestData.TestTeam.Generate();
        team.TenantId = TenantId;
        var listOfUsers = TestData.TestUser.Generate(5);
        team.Users = listOfUsers;
        await _dbContextMock.TenantUsers.AddRangeAsync(team.Users.Select(u => new TenantUser { UserId = u.Id, TenantId = TenantId }));
        await _dbContextMock.Teams.AddRangeAsync(team);
        await _dbContextMock.SaveChangesAsync();

        //Act
        var query = await _teamService.QueryTeamUsersAsync(team.Id);

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
    public async Task QueryTeamUsersAsync_TeamDoesNotExist_ShouldThrow()
    {
        //Arrange
        var team = TestData.TestTeam.Generate();
        team.TenantId = TenantId;
        await _dbContextMock.Teams.AddRangeAsync(team);
        await _dbContextMock.SaveChangesAsync();

        //Act
        var task = _teamService.QueryTeamUsersAsync(new Guid());

        //Assert
        task.Exception!.InnerException.Should().BeOfType<NotFoundException>();
    }

    [Fact]
    public async Task QueryTeamUsersAsync_UserIsFromDifferentTenant_ShouldThrow()
    {
        //Arrange
        var team = TestData.TestTeam.Generate();
        team.TenantId = TenantId;
        await _dbContextMock.Teams.AddRangeAsync(team);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock.Setup(x => x.TenantId).Returns(Guid.NewGuid());

        //Act
        var task = _teamService.QueryTeamUsersAsync(new Guid());

        //Assert
        task.Exception!.InnerException.Should().BeOfType<NotFoundException>();
    }
}
