using Bogus;
using FluentAssertions;
using Gridify;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL;
using TaxBeacon.DAL.Entities;
using TaxBeacon.DAL.Interceptors;
using TaxBeacon.DAL.Interfaces;
using TaxBeacon.UserManagement.Services;

namespace TaxBeacon.UserManagement.UnitTests.Services;

public class RoleServiceTests
{
    private readonly ITaxBeaconDbContext _dbContextMock;
    private readonly RoleService _roleService;
    private readonly Mock<EntitySaveChangesInterceptor> _entitySaveChangesInterceptorMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;

    public RoleServiceTests()
    {
        var logger = new Mock<ILogger<RoleService>>();
        var dateTimeService = new Mock<IDateTimeService>();
        _entitySaveChangesInterceptorMock = new();
        _dbContextMock = new TaxBeaconDbContext(
            new DbContextOptionsBuilder<TaxBeaconDbContext>()
                .UseInMemoryDatabase($"{nameof(RoleServiceTests)}-InMemoryDb-{Guid.NewGuid()}")
                .Options,
            _entitySaveChangesInterceptorMock.Object);

        _currentUserServiceMock = new();

        _roleService = new RoleService(_dbContextMock, logger.Object, dateTimeService.Object, _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task GetRolesAsync_AscendingOrderingAndPaginationWithSecondPage_CorrectNumberOfRolesInAscendingOrder()
    {
        //Arrange
        await TestData.SeedTestDataAsync(_dbContextMock, 2, 3);
        var tenantId = (await _dbContextMock.Tenants.FirstAsync()).Id;

        _currentUserServiceMock.Setup(s => s.TenantId).Returns(tenantId);

        var query = new GridifyQuery
        {
            Page = 2,
            PageSize = 2,
            OrderBy = "name asc"
        };

        //Act
        var pageOfUsers = await _roleService.GetRolesAsync(query);

        //Assert
        var listOfRoles = pageOfUsers.Query.ToList();
        listOfRoles.Count.Should().Be(1);
        listOfRoles.Select(x => x.Name).Should().BeInAscendingOrder((o1, o2) => string.Compare(o1, o2, StringComparison.InvariantCultureIgnoreCase));
        listOfRoles[0].AssignedUsersCount.Should().Be(2);
        pageOfUsers.Count.Should().Be(3);
    }

    [Fact]
    public async Task GetRolesAsync_PageNumberOutsideOfTotalRange_RoleListIsEmpty()
    {
        //Arrange
        await TestData.SeedTestDataAsync(_dbContextMock, 2, 3);
        var tenantId = (await _dbContextMock.Tenants.FirstAsync()).Id;

        _currentUserServiceMock.Setup(s => s.TenantId).Returns(tenantId);

        var query = new GridifyQuery
        {
            Page = 3,
            PageSize = 25,
            OrderBy = "name asc"
        };

        //Act
        var pageOfRoles = await _roleService.GetRolesAsync(query);

        //Assert
        pageOfRoles.Count.Should().Be(3);
        pageOfRoles.Query.Count().Should().Be(0);
    }

    [Fact]
    public async Task GetRoleAssignedUsersAsync_FirstPageAndAscendingOrder_CorrectNumberOfUsersInAscendingOrder()
    {
        //Arrange
        var role = await TestData.SeedTestDataForRoleAsync(_dbContextMock);
        var tenantId = (await _dbContextMock.Tenants.FirstAsync()).Id;

        _currentUserServiceMock.Setup(s => s.TenantId).Returns(tenantId);

        var query = new GridifyQuery
        {
            Page = 1,
            PageSize = 5,
            OrderBy = "email asc"
        };

        //Act
        var resultOneOf = await _roleService.GetRoleAssignedUsersAsync(role.Id, query);

        //Assert
        resultOneOf.TryPickT0(out var pageOfUsers, out _).Should().BeTrue();
        pageOfUsers.Count.Should().Be(5);
        pageOfUsers.Query.Count().Should().Be(5);
        var users = pageOfUsers.Query.ToList();
        users.Count.Should().Be(5);
        users.Select(x => x.Email).Should().BeInAscendingOrder((o1, o2) => string.Compare(o1, o2, StringComparison.InvariantCultureIgnoreCase));
    }

    [Fact]
    public async Task GetRoleAssignedUsersAsync_PageNumberOutsideOfTotalRange_NotFound()
    {
        //Arrange
        var role = await TestData.SeedTestDataForRoleAsync(_dbContextMock);
        var tenantId = (await _dbContextMock.Tenants.FirstAsync()).Id;

        _currentUserServiceMock.Setup(s => s.TenantId).Returns(tenantId);

        var query = new GridifyQuery
        {
            Page = 2,
            PageSize = 25,
            OrderBy = "email asc"
        };

        //Act
        var resultOneOf = await _roleService.GetRoleAssignedUsersAsync(role.Id, query);

        //Assert
        resultOneOf.TryPickT1(out var result, out _).Should().BeTrue();
    }

    [Fact]
    public async Task GetRoleAssignedUsersAsync_RoleIdDoesNotExist_NotFound()
    {
        //Arrange
        await TestData.SeedTestDataForRoleAsync(_dbContextMock);
        var tenantId = (await _dbContextMock.Tenants.FirstAsync()).Id;

        _currentUserServiceMock.Setup(s => s.TenantId).Returns(tenantId);

        var query = new GridifyQuery
        {
            Page = 1,
            PageSize = 25,
            OrderBy = "email asc"
        };

        //Act
        var resultOneOf = await _roleService.GetRoleAssignedUsersAsync(Guid.NewGuid(), query);

        //Assert
        resultOneOf.TryPickT1(out var result, out _).Should().BeTrue();
    }

    [Fact]
    public async Task UnassignUsersAsync_UnassignUsersFromRole_ShouldUnassignOnlyProvidedUsers()
    {
        //Arrange
        await TestData.SeedTestDataAsync(_dbContextMock, 3, 1);

        var users = await _dbContextMock.Users.ToListAsync();
        var tenant = await _dbContextMock.Tenants.FirstAsync();
        var role = await _dbContextMock.Roles.FirstAsync();
        _currentUserServiceMock.Setup(x => x.TenantId).Returns(tenant.Id);
        _currentUserServiceMock.Setup(x => x.UserId).Returns(users[2].Id);
        //Act
        await _roleService.UnassignUsersAsync(role.Id, new List<Guid>
        {
            users[0].Id, users[1].Id
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
        _currentUserServiceMock.Setup(x => x.TenantId).Returns(tenant.Id);
        var role = await _dbContextMock.Roles.FirstAsync();
        _currentUserServiceMock.Setup(x => x.UserId).Returns((await _dbContextMock.Users.LastAsync()).Id);
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
        _currentUserServiceMock.Setup(x => x.TenantId).Returns(tenant.Id);
        _currentUserServiceMock.Setup(x => x.UserId).Returns((await _dbContextMock.Users.LastAsync()).Id);
        //Act
        var result = await _roleService.UnassignUsersAsync(new Guid(), new List<Guid>(), default);

        //Assert
        result.IsT1.Should().BeTrue();
    }

    [Fact]
    public async Task AssignUsersAsync_ExistingRoleIdAndNewUsers_ShouldReturnSuccess()
    {
        // Arrange
        var tenant = TestData.TestTenant.Generate();
        var role = TestData.TestRole.Generate();
        var usersToAssign = TestData.TestUser.Generate(3);
        var currentUser = TestData.TestUser.Generate();

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Users.AddRangeAsync(usersToAssign);
        await _dbContextMock.Users.AddAsync(currentUser);
        await _dbContextMock.Roles.AddAsync(role);

        await _dbContextMock.TenantUsers.AddRangeAsync(usersToAssign.Select(user => new TenantUser
        {
            TenantId = tenant.Id,
            UserId = user.Id,
            User = user
        }));
        await _dbContextMock.TenantRoles.AddAsync(new TenantRole
        {
            TenantId = tenant.Id,
            RoleId = role.Id
        });
        await _dbContextMock.TenantUsers.AddAsync(new TenantUser
        {
            TenantId = tenant.Id,
            UserId = currentUser.Id,
            User = currentUser
        });
        await _dbContextMock.TenantUserRoles.AddAsync(new TenantUserRole
        {
            TenantId = tenant.Id,
            UserId = currentUser.Id,
            RoleId = role.Id
        });

        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock.Setup(x => x.UserId).Returns(currentUser.Id);
        _currentUserServiceMock.Setup(x => x.TenantId).Returns(tenant.Id);

        // Act
        var result = await _roleService.AssignUsersAsync(
            role.Id, usersToAssign.Select(x => x.Id).ToList(), default);

        // Assert
        result.IsT0.Should().BeTrue();
        _dbContextMock.TenantUserRoles.Count().Should().Be(4);
        _dbContextMock.UserActivityLogs.Count().Should().Be(3);
    }

    [Fact]
    public async Task AssignUsersAsync_NonExistingRoleId_ShouldReturnNotFound()
    {
        // Arrange
        var tenant = TestData.TestTenant.Generate();
        var role = TestData.TestRole.Generate();
        var usersToAssign = TestData.TestUser.Generate(3);
        var currentUser = TestData.TestUser.Generate();

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Users.AddRangeAsync(usersToAssign);
        await _dbContextMock.Users.AddAsync(currentUser);
        await _dbContextMock.Roles.AddAsync(role);
        await _dbContextMock.TenantRoles.AddAsync(new TenantRole
        {
            TenantId = tenant.Id,
            RoleId = role.Id
        });
        await _dbContextMock.TenantUsers.AddAsync(new TenantUser
        {
            TenantId = tenant.Id,
            UserId = currentUser.Id
        });
        await _dbContextMock.TenantUserRoles.AddAsync(new TenantUserRole
        {
            TenantId = tenant.Id,
            UserId = currentUser.Id,
            RoleId = role.Id
        });

        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock.Setup(x => x.UserId).Returns(currentUser.Id);
        _currentUserServiceMock.Setup(x => x.TenantId).Returns(tenant.Id);

        // Act
        var result = await _roleService.AssignUsersAsync(
            Guid.NewGuid(), usersToAssign.Select(x => x.Id).ToList(), default);

        // Assert
        result.IsT1.Should().BeTrue();
        _dbContextMock.TenantUserRoles.Count().Should().Be(1);
        _dbContextMock.UserActivityLogs.Count().Should().Be(0);
    }

    //TODO Refactor test data seeds
    private static class TestData
    {
        public static async Task SeedTestDataAsync(ITaxBeaconDbContext dbContext, int numberOfUsers, int numberOfRoles)
        {
            var tenant = TestTenant.Generate();
            var users = TestUser.Generate(numberOfUsers);
            var roles = TestRole.Generate(numberOfRoles);

            var listOfTenantUsers = new List<TenantUser>();
            users.ForEach(x => listOfTenantUsers.Add(new TenantUser
            {
                Tenant = tenant,
                User = x
            }));

            var listOfTenantRoles = new List<TenantRole>();
            roles.ForEach(x => listOfTenantRoles.Add(new TenantRole
            {
                Tenant = tenant,
                Role = x
            }));

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
            users.ForEach(x => listOfTenantUsers.Add(new TenantUser
            {
                Tenant = tenant,
                User = x
            }));

            var tenantRole = new TenantRole
            {
                Tenant = tenant,
                Role = role
            };

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

        public static readonly Faker<Tenant> TestTenant =
            new Faker<Tenant>()
                .RuleFor(t => t.Id, f => Guid.NewGuid())
                .RuleFor(t => t.Name, f => f.Company.CompanyName())
                .RuleFor(t => t.CreatedDateTimeUtc, f => DateTime.UtcNow);

        public static readonly Faker<User> TestUser =
            new Faker<User>()
                .RuleFor(u => u.Id, f => Guid.NewGuid())
                .RuleFor(u => u.FirstName, f => f.Name.FirstName())
                .RuleFor(u => u.LastName, f => f.Name.LastName())
                .RuleFor(u => u.LegalName, (_, u) => u.FirstName)
                .RuleFor(u => u.Email, f => f.Internet.Email())
                .RuleFor(u => u.CreatedDateTimeUtc, f => DateTime.UtcNow)
                .RuleFor(u => u.Status, f => f.PickRandom<Status>())
                .RuleFor(u => u.IsDeleted, f => false);

        public static readonly Faker<Role> TestRole =
            new Faker<Role>()
                .RuleFor(u => u.Id, f => Guid.NewGuid())
                .RuleFor(u => u.Name, f => f.Name.JobTitle());
    }
}
