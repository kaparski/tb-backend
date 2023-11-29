using FluentAssertions;
using System.Net.Mail;
using TaxBeacon.DAL.Administration.Entities;

namespace TaxBeacon.Administration.UnitTests.Services.Users;

public partial class UserServiceTests
{
    [Fact]
    public async Task GetUserByEmailAsync_ValidUserEmail_ReturnsUser()
    {
        //Arrange
        var tenant = UserServiceTests.TestData.TenantFaker.Generate();
        var users = UserServiceTests.TestData.UserFaker.Generate(5);
        var userEmail = users.First().Email;

        foreach (var user in users)
        {
            user.TenantUsers.Add(new TenantUser { Tenant = tenant });
        }

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Users.AddRangeAsync(users);
        await _dbContextMock.SaveChangesAsync();

        //Act
        var actualResult = await _userService.GetUserByEmailAsync(new MailAddress(userEmail));

        //Assert
        actualResult.TryPickT0(out var actualUser, out _).Should().BeTrue();
        actualUser.Email.Should().Be(userEmail);
    }
}
