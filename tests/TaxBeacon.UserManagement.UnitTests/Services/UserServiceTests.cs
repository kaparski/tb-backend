using Bogus;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using TaxBeacon.Common.Enums;
using TaxBeacon.DAL;
using TaxBeacon.DAL.Entities;
using TaxBeacon.DAL.Interceptors;
using TaxBeacon.DAL.Interfaces;
using TaxBeacon.UserManagement.Services;

namespace TaxBeacon.UserManagement.UnitTests.Services;

public class UserServiceTests
{
    private readonly Mock<ILogger<UserService>> _userServiceLoggerMock;
    private readonly Mock<EntitySaveChangesInterceptor> _entitySaveChangesInterceptorMock;
    private readonly ITaxBeaconDbContext _dbContextMock;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _userServiceLoggerMock = new();
        _entitySaveChangesInterceptorMock = new();

        _dbContextMock = new TaxBeaconDbContext(
            new DbContextOptionsBuilder<TaxBeaconDbContext>()
                .UseInMemoryDatabase($"{nameof(UserServiceTests)}-InMemoryDb-{Guid.NewGuid()}")
                .Options,
            _entitySaveChangesInterceptorMock.Object);

        _userService = new UserService(_userServiceLoggerMock.Object, _dbContextMock);
    }

    [Fact]
    public async Task LoginAsync_ValidEmailAndUserExist_LastLoginDateUpdated()
    {
        //Arrange
        var tenant = TestData.TestTenant.Generate();
        var user = TestData.TestUser.Generate();
        var currentDate = DateTime.UtcNow;

        user.TenantUsers.Add(new() { Tenant = tenant });
        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Users.AddAsync(user);
        await _dbContextMock.SaveChangesAsync();

        //Act
        await _userService.LoginAsync(user.Email);
        var actualResult = await _dbContextMock.Users.LastAsync();

        //Assert
        (await _dbContextMock.SaveChangesAsync()).Should().Be(0);
        actualResult.LastLoginDateUtc.Should().BeAfter(currentDate);
    }

    [Fact]
    public async Task LoginAsync_ValidEmailAndUserNotExist_NewUserCreated()
    {
        //Arrange
        var tenant = TestData.TestTenant.Generate();
        var user = TestData.TestUser.Generate();
        var currentDate = DateTime.UtcNow;

        _dbContextMock.Tenants.Add(tenant);
        await _dbContextMock.SaveChangesAsync();

        //Act
        await _userService.LoginAsync(user.Email);
        var actualResult = await _dbContextMock.Users.LastAsync();

        //Assert
        using (new AssertionScope())
        {
            (await _dbContextMock.SaveChangesAsync()).Should().Be(0);
            actualResult.Email.Should().Be(user.Email);
            actualResult.LastName.Should().BeEmpty();
            actualResult.FirstName.Should().BeEmpty();
            actualResult.Username.Should().BeEmpty();
            actualResult.TenantUsers.Should()
                .NotBeEmpty()
                .And
                .HaveCount(1);
            actualResult.TenantUsers.First().TenantId.Should().Be(tenant.Id);
            actualResult.LastLoginDateUtc.Should().BeAfter(currentDate);
        }
    }

    private static class TestData
    {
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
                .RuleFor(u => u.Username, (f, u) => f.Internet.UserName(u.FirstName, u.LastName))
                .RuleFor(u => u.Email, f => f.Internet.Email())
                .RuleFor(u => u.CreatedDateUtc, f => DateTime.UtcNow)
                .RuleFor(u => u.UserStatus, f => f.PickRandom<UserStatus>());
    }
}
