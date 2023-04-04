using Bogus;
using FluentAssertions;
using Gridify;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using OneOf.Types;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL;
using TaxBeacon.DAL.Entities;
using TaxBeacon.DAL.Interceptors;
using TaxBeacon.DAL.Interfaces;
using TaxBeacon.UserManagement.Models;
using TaxBeacon.UserManagement.Services;

namespace TaxBeacon.UserManagement.UnitTests.Services;

public class RoleServiceTests
{
    private readonly ITaxBeaconDbContext _dbContextMock;
    private readonly RoleService _roleService;
    private readonly Mock<EntitySaveChangesInterceptor> _entitySaveChangesInterceptorMock;
    private readonly Mock<ICurrentUserService> _currentUserService;

    public RoleServiceTests()
    {
        _currentUserService = new Mock<ICurrentUserService>();
        var logger = new Mock<ILogger<RoleService>>();
        var dateTimeService = new Mock<IDateTimeService>();
        _entitySaveChangesInterceptorMock = new();
        _dbContextMock = new TaxBeaconDbContext(
            new DbContextOptionsBuilder<TaxBeaconDbContext>()
                .UseInMemoryDatabase($"{nameof(RoleServiceTests)}-InMemoryDb-{Guid.NewGuid()}")
                .Options,
            _entitySaveChangesInterceptorMock.Object);

        _roleService = new RoleService(_dbContextMock, logger.Object, dateTimeService.Object, _currentUserService.Object);
    }

    [Fact]
    public async Task GetRolesAsync_DescendingOrderingAndPaginationWithSecondPage_CorrectNumberOfRolesInAscendingOrder()
    {
        //Arrange
        await TestData.SeedTestDataAsync(_dbContextMock, 2, 3);
        var tenantId = (await _dbContextMock.Tenants.FirstAsync()).Id;

        var query = new GridifyQuery { Page = 2, PageSize = 2, OrderBy = "name asc" };

        //Act
        var pageOfUsers = await _roleService.GetRolesAsync(tenantId, query);

        //Assert
        var listOfRoles = pageOfUsers.Query.ToList();
        listOfRoles.Count().Should().Be(1);
        listOfRoles.Select(x => x.Name).Should().BeInAscendingOrder();
        listOfRoles[0].AssignedUsersCount.Should().Be(2);
        pageOfUsers.Count.Should().Be(3);
    }

    [Fact]
    public async Task GetRolesAsync_PageNumberOutsideOfTotalRange_RoleListIsEmpty()
    {
        //Arrange
        await TestData.SeedTestDataAsync(_dbContextMock, 2, 3);
        var tenantId = (await _dbContextMock.Tenants.FirstAsync()).Id;

        var query = new GridifyQuery { Page = 3, PageSize = 25, OrderBy = "name asc", };

        //Act
        var pageOfRoles = await _roleService.GetRolesAsync(tenantId, query);

        //Assert
        pageOfRoles.Count.Should().Be(3);
        pageOfRoles.Query.Count().Should().Be(0);
    }

    [Fact]
    public async Task UnassignUsersAsync_UnassignUsersFromRole_ShouldUnassignOnlyProvidedUsers()
    {
        //Arrange
        await TestData.SeedTestDataAsync(_dbContextMock, 3, 1);

        var users = await _dbContextMock.Users.ToListAsync();
        var tenant = await _dbContextMock.Tenants.FirstAsync();
        var role = await _dbContextMock.Roles.FirstAsync();
        _currentUserService.Setup(x => x.TenantId).Returns(tenant.Id);
        _currentUserService.Setup(x => x.UserId).Returns(users[2].Id);
        //Act
        await _roleService.UnassignUsersAsync(role.Id, new List<Guid>
        {
            users[0].Id,
            users[1].Id
        }, default);

        //Assert
        _dbContextMock.TenantUserRoles.Count().Should().Be(1);
    }

    [Fact]
    public async Task UnassignUsersAsync_UnassignUsersFromRole_ShouldUnassignNoOne()
    {
        //Arrange
        await TestData.SeedTestDataAsync(_dbContextMock, 3, 1);

        var tenant = await _dbContextMock.Tenants.FirstAsync();
        _currentUserService.Setup(x => x.TenantId).Returns(tenant.Id);
        var role = await _dbContextMock.Roles.FirstAsync();
        _currentUserService.Setup(x => x.UserId).Returns((await _dbContextMock.Users.LastAsync()).Id);
        //Act
        await _roleService.UnassignUsersAsync(role.Id, new List<Guid>(), default);

        //Assert
        _dbContextMock.TenantUserRoles.Count().Should().Be(3);
    }

    [Fact]
    public async Task UnassignUsersAsync_IncorrectRoleId_ShouldReturnNotFound()
    {
        //Arrange
        await TestData.SeedTestDataAsync(_dbContextMock, 3, 1);

        var tenant = await _dbContextMock.Tenants.FirstAsync();
        _currentUserService.Setup(x => x.TenantId).Returns(tenant.Id);
        _currentUserService.Setup(x => x.UserId).Returns((await _dbContextMock.Users.LastAsync()).Id);
        //Act
        var result = await _roleService.UnassignUsersAsync(new Guid(), new List<Guid>(), default);

        //Assert
        result.IsT1.Should().BeTrue();
    }

    private static class TestData
    {
        public static async Task SeedTestDataAsync(ITaxBeaconDbContext dbContext, int numberOfUsers, int numberOfRoles)
        {
            var tenant = TestTenant.Generate();
            var users = TestUser.Generate(numberOfUsers);
            var roles = TestRole.Generate(numberOfRoles);

            var listOfTenantUsers = new List<TenantUser>();
            users.ForEach(x => listOfTenantUsers.Add(new TenantUser { Tenant = tenant, User = x }));

            var listOfTenantRoles = new List<TenantRole>();
            roles.ForEach(x => listOfTenantRoles.Add(new TenantRole { Tenant = tenant, Role = x }));

            var listOfTenantUserRoles = new List<TenantUserRole>();
            foreach (var tenantUser in listOfTenantUsers)
            {
                foreach (var tenantRole in listOfTenantRoles)
                {
                    listOfTenantUserRoles.Add(new TenantUserRole()
                    {
                        TenantRole = tenantRole,
                        TenantUser = tenantUser,
                    });
                }
            }

            await dbContext.Tenants.AddAsync(tenant);
            await dbContext.Users.AddRangeAsync(users);
            await dbContext.Roles.AddRangeAsync(roles);
            await dbContext.TenantUsers.AddRangeAsync(listOfTenantUsers);
            await dbContext.TenantRoles.AddRangeAsync(listOfTenantRoles);
            await dbContext.TenantUserRoles.AddRangeAsync(listOfTenantUserRoles);

            await dbContext.SaveChangesAsync();
        }

        private static readonly Faker<Tenant> TestTenant =
            new Faker<Tenant>()
                .RuleFor(t => t.Id, f => Guid.NewGuid())
                .RuleFor(t => t.Name, f => f.Company.CompanyName())
                .RuleFor(t => t.CreatedDateTimeUtc, f => DateTime.UtcNow);

        private static readonly Faker<User> TestUser =
            new Faker<User>()
                .RuleFor(u => u.Id, f => Guid.NewGuid())
                .RuleFor(u => u.FirstName, f => f.Name.FirstName())
                .RuleFor(u => u.LastName, f => f.Name.LastName())
                .RuleFor(u => u.Email, f => f.Internet.Email())
                .RuleFor(u => u.CreatedDateTimeUtc, f => DateTime.UtcNow)
                .RuleFor(u => u.Status, f => f.PickRandom<Status>());

        private static readonly Faker<Role> TestRole =
            new Faker<Role>()
                .RuleFor(u => u.Id, f => Guid.NewGuid())
                .RuleFor(u => u.Name, f => f.Name.JobTitle());
    }
}
