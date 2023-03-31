using Bogus;
using FluentAssertions;
using Gridify;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Moq;
using TaxBeacon.Common.Enums;
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

    public RoleServiceTests()
    {
        _entitySaveChangesInterceptorMock = new();
        _dbContextMock = new TaxBeaconDbContext(
            new DbContextOptionsBuilder<TaxBeaconDbContext>()
                .UseInMemoryDatabase($"{nameof(RoleServiceTests)}-InMemoryDb-{Guid.NewGuid()}")
                .Options,
            _entitySaveChangesInterceptorMock.Object);

        _roleService = new RoleService(_dbContextMock);
    }

    [Fact]
    public async Task GetRolesAsync_AscendingOrderingAndPaginationWithSecondPage_CorrectNumberOfRolesInAscendingOrder()
    {
        //Arrange
        await TestData.SeedTestDataAsync(_dbContextMock);
        var tenantId = (await _dbContextMock.Tenants.FirstAsync()).Id;

        var query = new GridifyQuery { Page = 2, PageSize = 2, OrderBy = "name asc" };

        //Act
        var pageOfUsers = await _roleService.GetRolesAsync(tenantId, query);

        //Assert
        var listOfRoles = pageOfUsers.Query.ToList();
        listOfRoles.Count.Should().Be(1);
        listOfRoles.Select(x => x.Name).Should().BeInAscendingOrder();
        listOfRoles[0].AssignedUsersCount.Should().Be(2);
        pageOfUsers.Count.Should().Be(3);
    }

    [Fact]
    public async Task GetRolesAsync_PageNumberOutsideOfTotalRange_RoleListIsEmpty()
    {
        //Arrange
        await TestData.SeedTestDataAsync(_dbContextMock);
        var tenantId = (await _dbContextMock.Tenants.FirstAsync()).Id;

        var query = new GridifyQuery { Page = 3, PageSize = 25, OrderBy = "name asc", };

        //Act
        var pageOfRoles = await _roleService.GetRolesAsync(tenantId, query);

        //Assert
        pageOfRoles.Count.Should().Be(3);
        pageOfRoles.Query.Count().Should().Be(0);
    }

    [Fact]
    public async Task GetRoleUsersAsync_FirstPageAndAscendingOrder_CorrentNumberOfUsersInAscendingOrder()
    {
        //Arrange
        var role = await TestData.SeedTestDataForRoleAsync(_dbContextMock);
        var tenantId = (await _dbContextMock.Tenants.FirstAsync()).Id;

        var query = new GridifyQuery { Page = 1, PageSize = 5, OrderBy = "email asc", };

        //Act
        var pageOfUsers = await _roleService.GetRoleUsersAsync(tenantId, role.Id, query);

        //Assert
        pageOfUsers.Count.Should().Be(5);
        pageOfUsers.Query.Count().Should().Be(5);
        var users = pageOfUsers.Query.ToList();
        users.Count.Should().Be(5);
        users.Select(x => x.Email).Should().BeInAscendingOrder();
    }

    [Fact]
    public async Task GetRoleUsersAsync_PageNumberOutsideOfTotalRange_UserListIsEmpty()
    {
        //Arrange
        var role = await TestData.SeedTestDataForRoleAsync(_dbContextMock);
        var tenantId = (await _dbContextMock.Tenants.FirstAsync()).Id;

        var query = new GridifyQuery { Page = 2, PageSize = 25, OrderBy = "email asc", };

        //Act
        var pageOfUsers = await _roleService.GetRoleUsersAsync(tenantId, role.Id, query);

        //Assert
        pageOfUsers.Count.Should().Be(5);
        pageOfUsers.Query.Count().Should().Be(0);
    }

    [Fact]
    public async Task GetRoleUsersAsync_RoleIdDoesNotExist_UserListIsEmpty()
    {
        //Arrange
        await TestData.SeedTestDataAsync(_dbContextMock);
        var tenantId = (await _dbContextMock.Tenants.FirstAsync()).Id;

        var query = new GridifyQuery { Page = 1, PageSize = 25, OrderBy = "email asc", };

        //Act
        var pageOfUsers = await _roleService.GetRoleUsersAsync(tenantId, Guid.NewGuid(), query);

        //Assert
        pageOfUsers.Count.Should().Be(0);
        pageOfUsers.Query.Count().Should().Be(0);
    }

    private static class TestData
    {
        public static async Task SeedTestDataAsync(ITaxBeaconDbContext dbContext)
        {
            var tenant = TestTenant.Generate();
            var users = TestUser.Generate(2);
            var roles = TestRole.Generate(3);

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

        public static async Task<Role> SeedTestDataForRoleAsync(ITaxBeaconDbContext dbContext)
        {
            var tenant = TestTenant.Generate();
            var users = TestUser.Generate(5);
            var role = TestRole.Generate();

            var listOfTenantUsers = new List<TenantUser>();
            users.ForEach(x => listOfTenantUsers.Add(new TenantUser { Tenant = tenant, User = x }));

            var tenantRole = new TenantRole { Tenant = tenant, Role = role };

            var listOfTenantUserRoles = new List<TenantUserRole>();
            foreach (var tenantUser in listOfTenantUsers)
            {
                listOfTenantUserRoles.Add(new TenantUserRole()
                {
                    TenantRole = tenantRole,
                    TenantUser = tenantUser,
                });
            }

            await dbContext.Tenants.AddAsync(tenant);
            await dbContext.Users.AddRangeAsync(users);
            await dbContext.Roles.AddAsync(role);
            await dbContext.TenantUsers.AddRangeAsync(listOfTenantUsers);
            await dbContext.TenantRoles.AddAsync(tenantRole);
            await dbContext.TenantUserRoles.AddRangeAsync(listOfTenantUserRoles);

            await dbContext.SaveChangesAsync();

            return role;
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
