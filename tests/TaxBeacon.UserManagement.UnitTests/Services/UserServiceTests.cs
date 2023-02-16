using Bogus;
using FluentAssertions;
using FluentAssertions.Execution;
using Gridify;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net.Mail;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL;
using TaxBeacon.DAL.Entities;
using TaxBeacon.DAL.Interceptors;
using TaxBeacon.DAL.Interfaces;
using TaxBeacon.UserManagement.Services;

namespace TaxBeacon.UserManagement.UnitTests.Services;

public class UserServiceTests
{
    private readonly Mock<IDateTimeService> _dateTimeServiceMock;
    private readonly Mock<EntitySaveChangesInterceptor> _entitySaveChangesInterceptorMock;
    private readonly Mock<ILogger<UserService>> _userServiceLoggerMock;
    private readonly ITaxBeaconDbContext _dbContextMock;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _userServiceLoggerMock = new();
        _entitySaveChangesInterceptorMock = new();
        _dateTimeServiceMock = new();

        _dbContextMock = new TaxBeaconDbContext(
            new DbContextOptionsBuilder<TaxBeaconDbContext>()
                .UseInMemoryDatabase($"{nameof(UserServiceTests)}-InMemoryDb-{Guid.NewGuid()}")
                .Options,
            _entitySaveChangesInterceptorMock.Object);

        _userService = new UserService(_userServiceLoggerMock.Object, _dbContextMock, _dateTimeServiceMock.Object);
    }

    [Fact]
    public async Task LoginAsync_ValidEmailAndUserExist_LastLoginDateUpdated()
    {
        //Arrange
        var tenant = TestData.TestTenant.Generate();
        var user = TestData.TestUser.Generate();
        var mailAddress = new MailAddress(user.Email);
        var currentDate = DateTime.UtcNow;

        user.TenantUsers.Add(new TenantUser { Tenant = tenant });
        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Users.AddAsync(user);
        await _dbContextMock.SaveChangesAsync();

        _dateTimeServiceMock
            .Setup(ds => ds.UtcNow)
            .Returns(DateTime.UtcNow);

        //Act
        await _userService.LoginAsync(mailAddress);
        var actualResult = await _dbContextMock.Users.LastAsync();

        //Assert
        (await _dbContextMock.SaveChangesAsync()).Should().Be(0);
        actualResult.LastLoginDateUtc.Should().BeAfter(currentDate);
        _dateTimeServiceMock
            .Verify(ds => ds.UtcNow, Times.Once);
    }

    [Fact]
    public async Task LoginAsync_ValidEmailAndUserNotExist_NewUserCreated()
    {
        //Arrange
        var tenant = TestData.TestTenant.Generate();
        var user = TestData.TestUser.Generate();
        var mailAddress = new MailAddress(user.Email);
        var currentDate = DateTime.UtcNow;

        _dbContextMock.Tenants.Add(tenant);
        await _dbContextMock.SaveChangesAsync();

        _dateTimeServiceMock
            .Setup(s => s.UtcNow)
            .Returns(currentDate);

        //Act
        await _userService.LoginAsync(mailAddress);
        var actualResult = await _dbContextMock.Users.LastAsync();

        //Assert
        using (new AssertionScope())
        {
            (await _dbContextMock.SaveChangesAsync()).Should().Be(0);
            actualResult.Email.Should().Be(user.Email);
            actualResult.LastName.Should().BeEmpty();
            actualResult.FirstName.Should().BeEmpty();
            actualResult.TenantUsers.Should()
                .NotBeEmpty()
                .And
                .HaveCount(1);
            actualResult.TenantUsers.First().TenantId.Should().Be(tenant.Id);
            actualResult.LastLoginDateUtc.Should().Be(currentDate);
            _dateTimeServiceMock
                .Verify(ds => ds.UtcNow, Times.Once);
        }
    }

    [Fact]
    public async Task GetUsersAsync_AscendingOrderingAndPaginationOfLastPage_AscendingOrderOfUsersAndCorrectPage()
    {
        // Arrange
        TestData.TestUser
            .RuleFor(u => u.Email, (f, u) => TestData.GetNextEmail());
        var users = TestData.TestUser.Generate(5);
        await _dbContextMock.Users.AddRangeAsync(users);
        await _dbContextMock.SaveChangesAsync();
        var query = new GridifyQuery
        {
            Page = 2,
            PageSize = 4,
            OrderBy = "email asc",
        };

        // Act
        var pageOfUsers = await _userService.GetUsersAsync(query, default);

        // Assert
        var listOfUsers = pageOfUsers.Query.ToList();
        listOfUsers.Count().Should().Be(1);
        listOfUsers[0].Email.Should().Be("abc@gmail.com");
        pageOfUsers.Count.Should().Be(5);
    }

    [Fact]
    public async Task GetUsersAsync_DescendingOrderingAndPaginationWithFirstPage_CorrectNumberOfUsersInDescendingOrder()
    {
        // Arrange
        TestData.TestUser
            .RuleFor(u => u.Email, (f, u) => TestData.GetNextEmail());
        var users = TestData.TestUser.Generate(6);
        await _dbContextMock.Users.AddRangeAsync(users);
        await _dbContextMock.SaveChangesAsync();
        var query = new GridifyQuery
        {
            Page = 1,
            PageSize = 25,
            OrderBy = "email desc",
        };

        // Act
        var pageOfUsers = await _userService.GetUsersAsync(query, default);

        // Assert
        var listOfUsers = pageOfUsers.Query.ToList();
        listOfUsers.Count().Should().Be(6);
        listOfUsers.Select(x => x.Email).Should().BeInDescendingOrder();
        pageOfUsers.Count.Should().Be(6);
    }

    [Fact]
    public async Task GetUsersAsync_PageNumberOutsideOfTotalRange_UserListIsEmpty()
    {
        // Arrange
        TestData.TestUser
            .RuleFor(u => u.Email, (f, u) => TestData.GetNextEmail());
        var users = TestData.TestUser.Generate(6);
        await _dbContextMock.Users.AddRangeAsync(users);
        await _dbContextMock.SaveChangesAsync();
        var query = new GridifyQuery
        {
            Page = 2,
            PageSize = 25,
            OrderBy = "email asc",
        };

        // Act
        var page = await _userService.GetUsersAsync(query, default);

        // Assert
        page.Count.Should().Be(6);
        var listOfUsers = page.Query.ToList();
        listOfUsers.Count().Should().Be(0);
    }

    [Fact]
    public async Task CreateAsync_ValidEmailAndUserNotExist_NewUserCreated()
    {
        //Arrange
        var tenant = TestData.TestTenant.Generate();
        var user = TestData.TestUser.Generate();
        var currentDate = DateTime.UtcNow;
        _dbContextMock.Tenants.Add(tenant);
        await _dbContextMock.SaveChangesAsync();

        //Act
        await _userService.CreateUserAsync(user);
        var actualResult = await _dbContextMock.Users.LastAsync();

        //Assert
        using (new AssertionScope())
        {
            (await _dbContextMock.SaveChangesAsync()).Should().Be(0);
            actualResult.Email.Should().Be(user.Email);
            actualResult.LastName.Should().Be(user.LastName);
            actualResult.FirstName.Should().Be(user.FirstName);
            actualResult.TenantUsers.Should()
                .NotBeEmpty()
                .And
                .HaveCount(1);
            actualResult.TenantUsers.First().TenantId.Should().Be(tenant.Id);
            actualResult.CreatedDateUtc.Should().Be(user.CreatedDateUtc);
        }
    }

    private static class TestData
    {
        public static List<string> CustomEmails = new()
        {
            "aaa@gmail.com",
            "abb@gmail.com",
            "abc@gmail.com"
        };

        static int _nameIndex = 0;

        public static readonly Faker<Tenant> TestTenant =
            new Faker<Tenant>()
                .RuleFor(t => t.Id, f => Guid.NewGuid())
                .RuleFor(t => t.Name, f => f.Company.CompanyName())
                .RuleFor(t => t.CreatedDateUtc, f => DateTime.UtcNow);

        public static readonly Faker<User> TestUser =
            new Faker<User>()
                .RuleFor(u => u.Id, f => Guid.NewGuid())
                .RuleFor(u => u.FirstName, f => f.Name.FirstName())
                .RuleFor(u => u.LastName, f => f.Name.LastName())
                .RuleFor(u => u.Email, f => f.Internet.Email())
                .RuleFor(u => u.CreatedDateUtc, f => DateTime.UtcNow)
                .RuleFor(u => u.UserStatus, f => f.PickRandom<UserStatus>());

        public static string GetNextEmail()
        {
            if (_nameIndex >= CustomEmails.Count)
                _nameIndex = 0;
            return CustomEmails[_nameIndex++];
        }
    }
}
