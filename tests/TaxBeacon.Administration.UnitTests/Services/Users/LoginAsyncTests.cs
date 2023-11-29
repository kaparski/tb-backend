using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Net.Mail;
using TaxBeacon.DAL.Administration.Entities;

namespace TaxBeacon.Administration.UnitTests.Services.Users;

public partial class UserServiceTests
{
    [Fact]
    public async Task LoginAsync_ValidObjectIdAndUserExist_ReturnsLoginUserDto()
    {
        //Arrange
        var user = TestData.UserFaker.Generate();
        var currentDate = DateTime.UtcNow;

        await _dbContextMock.Users.AddAsync(user);
        await _dbContextMock.SaveChangesAsync();

        _dateTimeServiceMock
            .Setup(ds => ds.UtcNow)
            .Returns(currentDate);

        //Act
        var actualResultOneOf = await _userService.LoginAsync(new MailAddress(user.Email));

        //Assert
        (await _dbContextMock.SaveChangesAsync()).Should().Be(0);
        var actualUser = await _dbContextMock.Users.LastAsync();
        actualUser.LastLoginDateTimeUtc.Should().Be(currentDate);

        actualResultOneOf.TryPickT0(out var loginUserDto, out _).Should().BeTrue();
        loginUserDto.UserId.Should().Be(user.Id);
        loginUserDto.FullName.Should().Be(user.FullName);
        loginUserDto.Permissions.Should().BeEquivalentTo(Enumerable.Empty<string>());
        loginUserDto.IsSuperAdmin.Should().BeFalse();

        _dateTimeServiceMock
            .Verify(ds => ds.UtcNow, Times.Once);
    }

    [Fact]
    public async Task LoginAsync_ValidObjectIdAndUserExist_TenantIdHintIsNotRespected()
    {
        //Arrange
        var user = TestData.UserFaker.Generate();
        var currentDate = DateTime.UtcNow;
        var tenant = TestData.TenantFaker.Generate();
        tenant.Id = _tenantId;
        tenant.DivisionEnabled = true;

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Users.AddAsync(user);
        await _dbContextMock.SaveChangesAsync();

        _dateTimeServiceMock
            .Setup(ds => ds.UtcNow)
            .Returns(currentDate);

        _currentUserServiceMock.SetupGet(m => m.TenantIdFromHeaders).Returns(_tenantId);

        //Act
        var actualResultOneOf = await _userService.LoginAsync(new MailAddress(user.Email));

        //Assert
        (await _dbContextMock.SaveChangesAsync()).Should().Be(0);
        var actualUser = await _dbContextMock.Users.LastAsync();
        actualUser.LastLoginDateTimeUtc.Should().Be(currentDate);

        actualResultOneOf.TryPickT0(out var loginUserDto, out _).Should().BeTrue();
        loginUserDto.UserId.Should().Be(user.Id);
        loginUserDto.FullName.Should().Be(user.FullName);
        loginUserDto.Permissions.Should().BeEquivalentTo(Enumerable.Empty<string>());
        loginUserDto.IsSuperAdmin.Should().BeFalse();
        loginUserDto.DivisionsEnabled.Should().NotBe(true);

        _dateTimeServiceMock
            .Verify(ds => ds.UtcNow, Times.Once);
    }

    [Fact]
    public async Task LoginAsync_UserHasMultipleTenants_ReturnsListOfTenants()
    {
        //Arrange
        var tenants = TestData.TenantFaker.RuleFor(t => t.Id, f => Guid.NewGuid()).Generate(3);

        foreach (var t in tenants)
        {
            t.TenantUsers.Add(new TenantUser { TenantId = t.Id, User = TestData.UserWithFixedEmailFaker.Generate() });
        }

        await _dbContextMock.Tenants.AddRangeAsync(tenants);
        await _dbContextMock.SaveChangesAsync();

        //Act
        var actualResultOneOf = await _userService.LoginAsync(new MailAddress(TestData.TestEmail));

        //Assert
        actualResultOneOf.TryPickT1(out var tenantDtos, out var _).Should().BeTrue();
    }

    [Fact]
    public async Task LoginAsync_ValidObjectIdAndUserExist_ReturnsSuperAdminLoginUserDto()
    {
        //Arrange
        var tenant = TestData.TenantFaker.Generate();
        tenant.Id = _tenantId;
        var user = TestData.UserFaker.Generate();
        var supeAdminRole = new Role { Id = Guid.NewGuid(), Name = "Super admin" };
        var currentDate = DateTime.UtcNow;

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Users.AddAsync(user);
        await _dbContextMock.Roles.AddAsync(supeAdminRole);
        await _dbContextMock.UserRoles.AddAsync(new UserRole { UserId = user.Id, RoleId = supeAdminRole.Id });
        await _dbContextMock.SaveChangesAsync();

        _dateTimeServiceMock
            .Setup(ds => ds.UtcNow)
            .Returns(currentDate);

        //Act
        var actualResultOneOf = await _userService.LoginAsync(new MailAddress(user.Email));

        //Assert
        using (new AssertionScope())
        {
            (await _dbContextMock.SaveChangesAsync()).Should().Be(0);
            var actualUser = await _dbContextMock.Users.FirstAsync(u => u.Id == user.Id);
            actualUser.LastLoginDateTimeUtc.Should().Be(currentDate);

            actualResultOneOf.TryPickT0(out var loginUserDto, out _).Should().BeTrue();
            loginUserDto.UserId.Should().Be(user.Id);
            loginUserDto.FullName.Should().Be(user.FullName);
            loginUserDto.Permissions.Should().BeEquivalentTo(Enumerable.Empty<string>());
            loginUserDto.IsSuperAdmin.Should().BeTrue();

            _dateTimeServiceMock
                .Verify(ds => ds.UtcNow, Times.Once);
        }
    }

    [Fact]
    public async Task LoginAsync_UserNotExist_ReturnsNotFound()
    {
        //Arrange
        var currentDate = DateTime.UtcNow;

        _dateTimeServiceMock
            .Setup(s => s.UtcNow)
            .Returns(currentDate);

        //Act
        var actualResultOneOf = await _userService.LoginAsync(new MailAddress("notexistes@mail.com"));

        //Assert
        using (new AssertionScope())
        {
            actualResultOneOf.IsT0.Should().BeFalse();
            actualResultOneOf.IsT1.Should().BeFalse();
            actualResultOneOf.IsT2.Should().BeTrue();
            _dateTimeServiceMock
                .Verify(ds => ds.UtcNow, Times.Never);
        }
    }
}
