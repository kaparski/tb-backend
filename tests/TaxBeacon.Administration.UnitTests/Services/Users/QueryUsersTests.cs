using FluentAssertions;
using FluentAssertions.Execution;
using System.Text.RegularExpressions;
using TaxBeacon.DAL.Administration.Entities;

namespace TaxBeacon.Administration.UnitTests.Services.Users;

public partial class UserServiceTests
{
    [Fact]
    public async Task QueryUsers_ReturnsNonTenantUsers()
    {
        // Arrange
        var userViews = TestData.UserViewFaker.Generate(5);

        await _dbContextMock.UsersView.AddRangeAsync(userViews);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(service => service.IsSuperAdmin)
            .Returns(true);

        _currentUserServiceMock
            .Setup(service => service.IsUserInTenant)
            .Returns(false);

        // Act
        var query = _userService.QueryUsers();
        var result = query.ToArray();

        // Assert

        using (new AssertionScope())
        {
            result.Should().HaveCount(5);

            foreach (var user in result)
            {
                var userView = userViews.Single(u => u.Id == user.Id);

                user.Should().BeEquivalentTo(userView, opt => opt.ExcludingMissingMembers());
            }
        }
    }

    [Fact]
    public async Task QueryUsers_ReturnsTenantUsers()
    {
        // Arrange
        var userViews = TestData.TenantUserViewFaker.Generate(5);

        userViews.ForEach(u => u.TenantId = TestData.TestTenantId);

        await _dbContextMock.TenantUsersView.AddRangeAsync(userViews);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(service => service.TenantId)
            .Returns(TestData.TestTenantId);

        _currentUserServiceMock
            .Setup(service => service.IsSuperAdmin)
            .Returns(false);

        // Act
        var query = _userService.QueryUsers();
        var result = query.ToArray();

        // Assert

        using (new AssertionScope())
        {
            result.Should().HaveCount(5);

            foreach (var user in result)
            {
                var userView = userViews.Single(u => u.Id == user.Id);

                user.Should().BeEquivalentTo(userView, opt => opt.ExcludingMissingMembers());
            }
        }
    }

    [Fact]
    public void UserView_ListOfColumnsMatchesUserViewEntity()
    {
        // Arrange

        var usersViewScript = File.ReadAllText("../../../../../migration-scripts/UsersView.sql");

        var fieldsAsString = new Regex(@"select((.|\n)*)from", RegexOptions.IgnoreCase | RegexOptions.Multiline)
            .Match(usersViewScript)
            .Groups[1]
            .Value;

        var fields = new Regex(@"(\w+),?[\r\n]", RegexOptions.IgnoreCase | RegexOptions.Multiline)
            .Matches(fieldsAsString)
            .Select(m => m.Groups[1].Value)
            .ToArray();

        var props = typeof(UserView).GetProperties()
            .Select(p => p.Name)
            .Where(p => p != "IsDeleted" && p != "DeletedDateTimeUtc")
            .ToArray();

        // Assert
        using (new AssertionScope())
        {
            foreach (var prop in props)
            {
                fields.Should().Contain(prop);
            }
        }
    }

    [Fact]
    public void TenantUserView_ListOfColumnsMatchesUserViewEntity()
    {
        // Arrange

        var usersViewScript = File.ReadAllText("../../../../../migration-scripts/TenantUsersView.sql");

        var fieldsAsString = new Regex(@"select((.|\n)*)from", RegexOptions.IgnoreCase | RegexOptions.Multiline)
            .Match(usersViewScript)
            .Groups[1]
            .Value;

        var fields = new Regex(@"(\w+),?[\r\n]", RegexOptions.IgnoreCase | RegexOptions.Multiline)
            .Matches(fieldsAsString)
            .Select(m => m.Groups[1].Value)
            .ToArray();

        var props = typeof(TenantUserView).GetProperties()
            .Select(p => p.Name)
            .Where(p => p != "IsDeleted" && p != "DeletedDateTimeUtc")
            .ToArray();

        // Assert
        using (new AssertionScope())
        {
            foreach (var prop in props)
            {
                fields.Should().Contain(prop);
            }
        }
    }
}
