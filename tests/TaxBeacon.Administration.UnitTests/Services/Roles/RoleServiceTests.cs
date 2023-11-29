using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using TaxBeacon.Administration.Roles;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL.Administration;
using TaxBeacon.DAL.Interceptors;
using TaxBeacon.DAL;
using Bogus;
using TaxBeacon.DAL.Administration.Entities;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.Administration.UnitTests.Services.Roles;
public partial class RoleServiceTests
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
