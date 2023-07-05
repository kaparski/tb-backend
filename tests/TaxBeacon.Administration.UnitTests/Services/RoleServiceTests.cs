using Bogus;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.Diagnostics.CodeAnalysis;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL;
using TaxBeacon.DAL.Interceptors;
using TaxBeacon.Administration.Roles;
using TaxBeacon.DAL.Administration;
using TaxBeacon.DAL.Administration.Entities;

namespace TaxBeacon.Administration.UnitTests.Services;

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

        _roleService = new RoleService(
            _dbContextMock,
            logger.Object,
            dateTimeService.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task UnassignUsersAsync_UserInTenantAndUserIds_ReturnsSuccess()
    {
        //Arrange
        await TestData.SeedTenantRolesAsync(_dbContextMock, 1, 3);

        var users = await _dbContextMock.Users.ToListAsync();
        var tenant = await _dbContextMock.Tenants.FirstAsync();
        var role = await _dbContextMock.Roles.FirstAsync();

        _currentUserServiceMock
            .Setup(x => x.TenantId)
            .Returns(tenant.Id);
        _currentUserServiceMock
            .Setup(x => x.IsSuperAdmin)
            .Returns(false);

        //Act
        var actualResult = await _roleService.UnassignUsersAsync(role.Id, new List<Guid>
        {
            users[0].Id, users[1].Id
        });

        //Assert
        using (new AssertionScope())
        {
            actualResult.IsT0.Should().BeTrue();
            _dbContextMock.TenantUserRoles.Count().Should().Be(1);
            _dbContextMock.UserActivityLogs.Count().Should().Be(2);
        }
    }

    [Fact]
    public async Task UnassignUsersAsync_SuperAdminWithoutTenantAndUserIds_ReturnsSuccess()
    {
        //Arrange
        await TestData.SeedNotTenantRolesAsync(_dbContextMock, 1, 3);

        var users = await _dbContextMock.Users.ToListAsync();
        var role = await _dbContextMock.Roles.FirstAsync();

        _currentUserServiceMock
            .Setup(x => x.TenantId)
            .Returns(Guid.Empty);
        _currentUserServiceMock
            .Setup(x => x.IsSuperAdmin)
            .Returns(true);

        //Act
        var actualResult = await _roleService.UnassignUsersAsync(role.Id, new List<Guid>
        {
            users[0].Id, users[1].Id
        });

        //Assert
        using (new AssertionScope())
        {
            actualResult.IsT0.Should().BeTrue();
            _dbContextMock.UserRoles.Count().Should().Be(1);
            _dbContextMock.UserActivityLogs.Count().Should().Be(2);
        }
    }

    [Fact]
    public async Task UnassignUsersAsync_IncorrectRoleIdAndUserInTenant_ReturnsNotFound()
    {
        //Arrange
        await TestData.SeedTenantRolesAsync(_dbContextMock, 1, 3);
        var tenant = await _dbContextMock.Tenants.FirstAsync();

        _currentUserServiceMock
            .Setup(x => x.TenantId)
            .Returns(tenant.Id);
        _currentUserServiceMock
            .Setup(x => x.IsSuperAdmin)
            .Returns(false);

        //Act
        var result = await _roleService.UnassignUsersAsync(new Guid(), new List<Guid>());

        //Assert
        result.IsT1.Should().BeTrue();
    }

    [Fact]
    public async Task UnassignUsersAsync_IncorrectRoleIdAndSuperAdminWithoutTenant_ReturnsNotFound()
    {
        //Arrange
        await TestData.SeedNotTenantRolesAsync(_dbContextMock, 1, 3);

        _currentUserServiceMock
            .Setup(x => x.TenantId)
            .Returns(Guid.Empty);
        _currentUserServiceMock
            .Setup(x => x.IsSuperAdmin)
            .Returns(true);

        //Act
        var result = await _roleService.UnassignUsersAsync(new Guid(), new List<Guid>());

        //Assert
        result.IsT1.Should().BeTrue();
    }

    [Fact]
    public async Task AssignUsersAsync_ExistingRoleIdAndNewUsersAndUserInTenant_ReturnsSuccess()
    {
        // Arrange
        var roles = await TestData.SeedTenantRolesAsync(_dbContextMock, 1, 3);
        var tenant = await _dbContextMock.Tenants.FirstAsync();
        var usersToAssign = TestData.UserFaker.Generate(3);

        await _dbContextMock.Users.AddRangeAsync(usersToAssign);
        await _dbContextMock.TenantUsers.AddRangeAsync(usersToAssign.Select(u => new TenantUser
        {
            TenantId = tenant.Id,
            UserId = u.Id
        }));
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(x => x.TenantId)
            .Returns(tenant.Id);
        _currentUserServiceMock
            .Setup(x => x.IsSuperAdmin)
            .Returns(false);

        // Act
        var result = await _roleService.AssignUsersAsync(
            roles[0].Id, usersToAssign.Select(x => x.Id).ToList());

        // Assert
        using (new AssertionScope())
        {
            result.IsT0.Should().BeTrue();
            _dbContextMock.TenantUserRoles.Count().Should().Be(6);
            _dbContextMock.UserRoles.Count().Should().Be(0);
            _dbContextMock.UserActivityLogs.Count().Should().Be(3);
        }
    }

    [Fact]
    public async Task AssignUsersAsync_ExistingRoleIdAndNewUsersAndSuperAdminWithoutTenant_ReturnsSuccess()
    {
        // Arrange
        var roles = await TestData.SeedNotTenantRolesAsync(_dbContextMock, 1, 3);
        var usersToAssign = TestData.UserFaker.Generate(3);

        await _dbContextMock.Users.AddRangeAsync(usersToAssign);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(x => x.TenantId)
            .Returns(Guid.Empty);
        _currentUserServiceMock
            .Setup(x => x.IsSuperAdmin)
            .Returns(true);

        // Act
        var result = await _roleService.AssignUsersAsync(
            roles[0].Id, usersToAssign.Select(x => x.Id).ToList());

        // Assert
        using (new AssertionScope())
        {
            result.IsT0.Should().BeTrue();
            _dbContextMock.UserRoles.Count().Should().Be(6);
            _dbContextMock.TenantUserRoles.Count().Should().Be(0);
            _dbContextMock.UserActivityLogs.Count().Should().Be(3);
        }
    }

    [Fact]
    public async Task AssignUsersAsync_NonExistingRoleIdAndUserInTenant_ShouldReturnNotFound()
    {
        // Arrange
        await TestData.SeedTenantRolesAsync(_dbContextMock, 1, 3);
        var tenant = await _dbContextMock.Tenants.FirstAsync();
        var usersToAssign = TestData.UserFaker.Generate(3);

        await _dbContextMock.Users.AddRangeAsync(usersToAssign);
        await _dbContextMock.TenantUsers.AddRangeAsync(usersToAssign.Select(u => new TenantUser
        {
            TenantId = tenant.Id,
            UserId = u.Id
        }));
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(x => x.TenantId)
            .Returns(tenant.Id);
        _currentUserServiceMock
            .Setup(x => x.IsSuperAdmin)
            .Returns(false);

        // Act
        var result = await _roleService.AssignUsersAsync(
            Guid.NewGuid(), usersToAssign.Select(x => x.Id).ToList());

        // Assert
        using (new AssertionScope())
        {
            result.IsT1.Should().BeTrue();
            _dbContextMock.TenantUserRoles.Count().Should().Be(3);
            _dbContextMock.UserRoles.Count().Should().Be(0);
            _dbContextMock.UserActivityLogs.Count().Should().Be(0);
        }
    }

    [Fact]
    public async Task AssignUsersAsync_NonExistingRoleIdAndSuperAdminWithoutTenant_ShouldReturnNotFound()
    {
        // Arrange
        await TestData.SeedNotTenantRolesAsync(_dbContextMock, 1, 3);
        var usersToAssign = TestData.UserFaker.Generate(3);

        await _dbContextMock.Users.AddRangeAsync(usersToAssign);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(x => x.TenantId)
            .Returns(Guid.Empty);
        _currentUserServiceMock
            .Setup(x => x.IsSuperAdmin)
            .Returns(true);

        // Act
        var result = await _roleService.AssignUsersAsync(
            Guid.NewGuid(), usersToAssign.Select(x => x.Id).ToList());

        // Assert
        using (new AssertionScope())
        {
            result.IsT1.Should().BeTrue();
            _dbContextMock.UserRoles.Count().Should().Be(3);
            _dbContextMock.TenantUserRoles.Count().Should().Be(0);
            _dbContextMock.UserActivityLogs.Count().Should().Be(0);
        }
    }

    [Fact]
    public async Task GetRolePermissionsByIdAsync_ExistingRoleIdAndUserInTenant_ReturnsRolePermissions()
    {
        // Arrange
        var roles = await TestData.SeedTenantRolesAsync(_dbContextMock, 1, 3);
        var tenant = await _dbContextMock.Tenants.FirstAsync();
        var permissions = new Faker().Random
            .WordsArray(4)
            .Select(word => new Permission
            {
                Id = Guid.NewGuid(),
                Name = word
            })
            .ToList();

        await _dbContextMock.TenantPermissions.AddRangeAsync(permissions
            .Select(permission => new TenantPermission
            {
                Tenant = tenant,
                Permission = permission
            }));

        await _dbContextMock.TenantRolePermissions.AddRangeAsync(permissions
            .Select(permission => new TenantRolePermission
            {
                TenantId = tenant.Id,
                RoleId = roles[0].Id,
                PermissionId = permission.Id
            }));
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(service => service.TenantId)
            .Returns(tenant.Id);
        _currentUserServiceMock
            .Setup(service => service.IsSuperAdmin)
            .Returns(false);

        // Act
        var actualResult = await _roleService.GetRolePermissionsByIdAsync(roles[0].Id);

        // Assert
        using (new AssertionScope())
        {
            actualResult.TryPickT0(out var actualPermissions, out _).Should().BeTrue();
            actualPermissions.Select(p => p.Id).Should().BeEquivalentTo(permissions.Select(perm => perm.Id));
        }
    }

    [Fact]
    public async Task GetRolePermissionsByIdAsync_ExistingRoleIdAndSuperAdminWithoutTenant_ReturnsRolePermissions()
    {
        // Arrange
        var roles = await TestData.SeedNotTenantRolesAsync(_dbContextMock, 1, 3);
        var permissions = new Faker().Random
            .WordsArray(4)
            .Select(word => new Permission
            {
                Id = Guid.NewGuid(),
                Name = word
            }).ToList();

        await _dbContextMock.RolePermissions.AddRangeAsync(permissions
            .Select(permission => new RolePermission
            {
                Role = roles[0],
                Permission = permission
            }));
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(service => service.TenantId)
            .Returns(Guid.Empty);
        _currentUserServiceMock
            .Setup(service => service.IsSuperAdmin)
            .Returns(true);

        // Act
        var actualResult = await _roleService.GetRolePermissionsByIdAsync(roles[0].Id);

        // Assert
        using (new AssertionScope())
        {
            actualResult.TryPickT0(out var actualPermissions, out _).Should().BeTrue();
            actualPermissions.Select(p => p.Id).Should().BeEquivalentTo(permissions.Select(perm => perm.Id));
        }
    }

    [Fact]
    public async Task GetRolePermissionsByIdAsync_NotExistingRoleIdAndUserInTenant_ReturnsNotFound()
    {
        // Arrange
        _currentUserServiceMock
            .Setup(service => service.TenantId)
            .Returns(Guid.NewGuid());
        _currentUserServiceMock
            .Setup(service => service.IsSuperAdmin)
            .Returns(false);

        // Act
        var actualResult = await _roleService.GetRolePermissionsByIdAsync(Guid.NewGuid());

        // Assert
        actualResult.IsT1.Should().BeTrue();
    }

    [Fact]
    public async Task GetRolePermissionsByIdAsync_NonExistingRoleIdAndSuperAdminWithoutTenant_ReturnsNotFound()
    {
        // Arrange
        _currentUserServiceMock
            .Setup(service => service.TenantId)
            .Returns(Guid.Empty);
        _currentUserServiceMock
            .Setup(service => service.IsSuperAdmin)
            .Returns(true);

        // Act
        var actualResult = await _roleService.GetRolePermissionsByIdAsync(Guid.NewGuid());

        // Assert
        actualResult.IsT1.Should().BeTrue();
    }

    [Fact]
    public async Task QueryRoles_ReturnsTenantRoles()
    {
        //Arrange
        var items = await TestData.SeedTenantRolesAsync(_dbContextMock, 3, 2);
        var tenantId = (await _dbContextMock.Tenants.FirstAsync()).Id;

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenantId);
        _currentUserServiceMock
            .Setup(s => s.IsUserInTenant)
            .Returns(true);
        _currentUserServiceMock
            .Setup(s => s.IsSuperAdmin)
            .Returns(false);

        // Act
        var query = _roleService.QueryRoles();
        var result = query.ToArray();

        // Assert

        using (new AssertionScope())
        {
            result.Should().HaveCount(3);

            foreach (var dto in result)
            {
                var item = items.Single(u => u.Id == dto.Id);

                dto.Should().BeEquivalentTo(item, opt => opt.ExcludingMissingMembers());

                dto.AssignedUsersCount.Should().Be(2);
            }
        }
    }

    [Fact]
    public async Task QueryRoles_ReturnsNonTenantRoles()
    {
        //Arrange
        var items = await TestData.SeedNotTenantRolesAsync(_dbContextMock, 3, 2);

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(Guid.Empty);
        _currentUserServiceMock
            .Setup(s => s.IsUserInTenant)
            .Returns(false);
        _currentUserServiceMock
            .Setup(s => s.IsSuperAdmin)
            .Returns(true);

        // Act
        var query = _roleService.QueryRoles();
        var result = query.ToArray();

        // Assert

        using (new AssertionScope())
        {
            result.Should().HaveCount(3);

            foreach (var dto in result)
            {
                var item = items.Single(u => u.Id == dto.Id);

                dto.Should().BeEquivalentTo(item, opt => opt.ExcludingMissingMembers());

                dto.AssignedUsersCount.Should().Be(2);
            }
        }
    }

    [Fact]
    public async Task QueryRoleAssignedUsersAsync_ReturnsTenantAssignedUsers()
    {
        //Arrange
        var roles = await TestData.SeedTenantRolesAsync(_dbContextMock);
        var tenantId = (await _dbContextMock.Tenants.FirstAsync()).Id;

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenantId);
        _currentUserServiceMock
            .Setup(s => s.IsSuperAdmin)
            .Returns(false);

        //Act
        var query = await _roleService.QueryRoleAssignedUsersAsync(roles[0].Id);
        var result = query.ToArray();

        //Assert
        using (new AssertionScope())
        {
            result.Should().HaveCount(5);

            foreach (var dto in result)
            {
                var item = _dbContextMock.Users.Single(u => u.Id == dto.Id);

                dto.Should().BeEquivalentTo(item, opt => opt.ExcludingMissingMembers());
            }
        }
    }

    [Fact]
    public async Task QueryRoleAssignedUsersAsync_ReturnsNonTenantAssignedUsers()
    {
        //Arrange
        var roles = await TestData.SeedNotTenantRolesAsync(_dbContextMock);

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(Guid.Empty);
        _currentUserServiceMock
            .Setup(s => s.IsUserInTenant)
            .Returns(false);
        _currentUserServiceMock
            .Setup(s => s.IsSuperAdmin)
            .Returns(true);

        //Act
        var query = await _roleService.QueryRoleAssignedUsersAsync(roles[0].Id);
        var result = query.ToArray();

        //Assert
        using (new AssertionScope())
        {
            result.Should().HaveCount(5);

            foreach (var dto in result)
            {
                var item = _dbContextMock.Users.Single(u => u.Id == dto.Id);

                dto.Should().BeEquivalentTo(item, opt => opt.ExcludingMissingMembers());
            }
        }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    private static class TestData
    {
        public static async Task<List<Role>> SeedNotTenantRolesAsync(ITaxBeaconDbContext dbContext,
            int numberOfRoles = 1,
            int numberOfUsers = 5)
        {
            var users = UserFaker.Generate(numberOfUsers);
            var roles = RoleFaker
                .RuleFor(r => r.Type, _ => SourceType.System)
                .Generate(numberOfRoles);

            var userRoles =
                (from user in users
                 from role in roles
                 select new UserRole
                 {
                     User = user,
                     Role = role,
                 }).ToList();

            await dbContext.Users.AddRangeAsync(users);
            await dbContext.Roles.AddRangeAsync(roles);
            await dbContext.UserRoles.AddRangeAsync(userRoles);

            await dbContext.SaveChangesAsync();

            return roles;
        }

        public static async Task<List<Role>> SeedTenantRolesAsync(ITaxBeaconDbContext dbContext,
            int numberOfRoles = 1,
            int numberOfUsers = 5)
        {
            var tenant = TenantFaker.Generate();
            var users = UserFaker.Generate(numberOfUsers);
            var roles = RoleFaker
                .RuleFor(r => r.Type, _ => SourceType.Tenant)
                .Generate(numberOfRoles);

            var listOfTenantUsers = users
                .Select(u => new TenantUser
                {
                    UserId = u.Id,
                    TenantId = tenant.Id
                }).ToList();

            var listOfTenantRoles = roles
                .Select(r => new TenantRole
                {
                    TenantId = tenant.Id,
                    RoleId = r.Id
                }).ToList();

            var listOfTenantUserRoles =
                (from tenantUser in listOfTenantUsers
                 from tenantRole in listOfTenantRoles
                 select new TenantUserRole
                 {
                     TenantRole = tenantRole,
                     TenantUser = tenantUser,
                 }).ToList();

            await dbContext.Tenants.AddAsync(tenant);
            await dbContext.Users.AddRangeAsync(users);
            await dbContext.Roles.AddRangeAsync(roles);
            await dbContext.TenantUsers.AddRangeAsync(listOfTenantUsers);
            await dbContext.TenantRoles.AddRangeAsync(listOfTenantRoles);
            await dbContext.TenantUserRoles.AddRangeAsync(listOfTenantUserRoles);

            await dbContext.SaveChangesAsync();

            return roles;
        }

        private static readonly Faker<Tenant> TenantFaker =
            new Faker<Tenant>()
                .RuleFor(t => t.Id, _ => Guid.NewGuid())
                .RuleFor(t => t.Name, f => f.Company.CompanyName())
                .RuleFor(t => t.CreatedDateTimeUtc, _ => DateTime.UtcNow);

        public static readonly Faker<User> UserFaker =
            new Faker<User>()
                .RuleFor(u => u.Id, _ => Guid.NewGuid())
                .RuleFor(u => u.FirstName, f => f.Name.FirstName())
                .RuleFor(u => u.LastName, f => f.Name.LastName())
                .RuleFor(u => u.LegalName, (_, u) => u.FirstName)
                .RuleFor(u => u.Email, f => f.Internet.Email())
                .RuleFor(u => u.CreatedDateTimeUtc, _ => DateTime.UtcNow)
                .RuleFor(u => u.Status, f => f.PickRandom<Status>())
                .RuleFor(u => u.IsDeleted, _ => false);

        private static readonly Faker<Role> RoleFaker =
            new Faker<Role>()
                .RuleFor(r => r.Id, _ => Guid.NewGuid())
                .RuleFor(r => r.Name, f => f.Name.JobTitle())
                .RuleFor(r => r.Type, f => f.PickRandom<SourceType>());
    }
}
