using Bogus;
using FluentAssertions;
using FluentAssertions.Execution;
using Gridify;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Net.Mail;
using System.Reflection;
using TaxBeacon.Common.Converters;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Exceptions;
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
        TypeAdapterConfig.GlobalSettings.Scan(typeof(RoleMappingConfig).Assembly);

        _entitySaveChangesInterceptorMock = new();
        _dbContextMock = new TaxBeaconDbContext(
            new DbContextOptionsBuilder<TaxBeaconDbContext>()
                .UseInMemoryDatabase($"{nameof(RoleServiceTests)}-InMemoryDb-{Guid.NewGuid()}")
                .Options,
            _entitySaveChangesInterceptorMock.Object);

        _roleService = new RoleService(_dbContextMock);
    }

    [Fact]
    public async Task GetRolesAsync_DescendingOrderingAndPaginationWithSecondPage_CorrectNumberOfRolesInAscendingOrder()
    {
        //Arrange
        var tenant = TestData.TestTenant.Generate();
        var users = TestData.TestUser.Generate(2);
        var roles = TestData.TestRole.Generate(3);

        var listOfTenantUsers = new List<TenantUser>();
        var listOfTenantRoles = new List<TenantRole>();
        users.ForEach(x => listOfTenantUsers.Add(new TenantUser { Tenant = tenant, User = x }));
        roles.ForEach(x => listOfTenantRoles.Add(new TenantRole { Tenant = tenant, Role = x }));

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Users.AddRangeAsync(users);
        await _dbContextMock.Roles.AddRangeAsync(roles);
        await _dbContextMock.TenantUsers.AddRangeAsync(listOfTenantUsers);
        await _dbContextMock.TenantRoles.AddRangeAsync(listOfTenantRoles);

        foreach (var tenantUser in listOfTenantUsers)
        {
            foreach (var tenantRole in listOfTenantRoles)
            {
                await _dbContextMock.TenantUserRoles.AddAsync(new TenantUserRole()
                {
                    TenantRole = tenantRole,
                    TenantUser = tenantUser,
                });
            }
        }

        var query = new GridifyQuery { Page = 2, PageSize = 2, OrderBy = "name asc" };
        await _dbContextMock.SaveChangesAsync();

        //Act
        var pageOfUsers = await _roleService.GetRolesAsync(query, default);

        //Assert
        var listOfRoles = pageOfUsers.Query.ToList();
        listOfRoles.Count().Should().Be(1);
        listOfRoles.Select(x => x.Name).Should().BeInAscendingOrder();
        listOfRoles[0].NumberOfUsers.Should().Be(2);
        pageOfUsers.Count.Should().Be(3);
    }

    [Fact]
    public async Task GetRolesAsync_PageNumberOutsideOfTotalRange_RoleListIsEmpty()
    {
        //Arrange
        var tenant = TestData.TestTenant.Generate();
        var users = TestData.TestUser.Generate(2);
        var roles = TestData.TestRole.Generate(3);

        var listOfTenantUsers = new List<TenantUser>();
        var listOfTenantRoles = new List<TenantRole>();
        users.ForEach(x => listOfTenantUsers.Add(new TenantUser { Tenant = tenant, User = x }));
        roles.ForEach(x => listOfTenantRoles.Add(new TenantRole { Tenant = tenant, Role = x }));

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Users.AddRangeAsync(users);
        await _dbContextMock.Roles.AddRangeAsync(roles);
        await _dbContextMock.TenantUsers.AddRangeAsync(listOfTenantUsers);
        await _dbContextMock.TenantRoles.AddRangeAsync(listOfTenantRoles);

        foreach (var tenantUser in listOfTenantUsers)
        {
            foreach (var tenantRole in listOfTenantRoles)
            {
                await _dbContextMock.TenantUserRoles.AddAsync(new TenantUserRole()
                {
                    TenantRole = tenantRole,
                    TenantUser = tenantUser,
                });
            }
        }

        await _dbContextMock.SaveChangesAsync();
        var query = new GridifyQuery { Page = 3, PageSize = 25, OrderBy = "name asc", };

        //Act
        var pageOfRoles = await _roleService.GetRolesAsync(query, default);

        //Assert
        pageOfRoles.Count.Should().Be(3);
        pageOfRoles.Query.Count().Should().Be(0);
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
                .RuleFor(u => u.Email, f => f.Internet.Email())
                .RuleFor(u => u.CreatedDateUtc, f => DateTime.UtcNow)
                .RuleFor(u => u.UserStatus, f => f.PickRandom<UserStatus>());

        public static readonly Faker<Role> TestRole =
            new Faker<Role>()
                .RuleFor(u => u.Id, f => Guid.NewGuid())
                .RuleFor(u => u.Name, f => f.Name.JobTitle());
    }
}
