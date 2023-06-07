using Microsoft.EntityFrameworkCore;
using System.Reflection;
using TaxBeacon.DAL.Entities.Accounts;
using TaxBeacon.DAL.Interceptors;
using TaxBeacon.DAL.Interfaces;

namespace TaxBeacon.DAL;

public class TaxBeaconDbContext: DbContext, ITaxBeaconDbContext, IAccountDbContext
{
    private readonly EntitySaveChangesInterceptor _saveChangesInterceptor;

    public TaxBeaconDbContext(
        DbContextOptions<TaxBeaconDbContext> options,
        EntitySaveChangesInterceptor saveChangesInterceptor)
        : base(options) => _saveChangesInterceptor = saveChangesInterceptor;

    public DbSet<Tenant> Tenants => Set<Tenant>();

    public DbSet<User> Users => Set<User>();

    public DbSet<TenantUser> TenantUsers => Set<TenantUser>();

    public DbSet<Permission> Permissions => Set<Permission>();

    public DbSet<TenantPermission> TenantPermissions => Set<TenantPermission>();

    public DbSet<Role> Roles => Set<Role>();

    public DbSet<TenantRole> TenantRoles => Set<TenantRole>();

    public DbSet<TenantRolePermission> TenantRolePermissions => Set<TenantRolePermission>();

    public DbSet<TenantUserRole> TenantUserRoles => Set<TenantUserRole>();

    public DbSet<UserActivityLog> UserActivityLogs => Set<UserActivityLog>();

    public DbSet<TableFilter> TableFilters => Set<TableFilter>();

    public DbSet<UserRole> UserRoles => Set<UserRole>();

    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    public DbSet<Division> Divisions => Set<Division>();

    public DbSet<Department> Departments => Set<Department>();

    public DbSet<ServiceArea> ServiceAreas => Set<ServiceArea>();

    public DbSet<Team> Teams => Set<Team>();

    public DbSet<JobTitle> JobTitles => Set<JobTitle>();

    public DbSet<DivisionActivityLog> DivisionActivityLogs => Set<DivisionActivityLog>();

    public DbSet<TeamActivityLog> TeamActivityLogs => Set<TeamActivityLog>();

    public DbSet<TenantActivityLog> TenantActivityLogs => Set<TenantActivityLog>();

    public DbSet<DepartmentActivityLog> DepartmentActivityLogs => Set<DepartmentActivityLog>();

    public DbSet<ServiceAreaActivityLog> ServiceAreaActivityLogs => Set<ServiceAreaActivityLog>();

    public DbSet<JobTitleActivityLog> JobTitleActivityLogs => Set<JobTitleActivityLog>();

    public DbSet<Program> Programs => Set<Program>();

    public DbSet<TenantProgram> TenantsPrograms => Set<TenantProgram>();

    public DbSet<ProgramActivityLog> ProgramActivityLogs => Set<ProgramActivityLog>();

    public DbSet<UserView> UsersView => Set<UserView>();

    public DbSet<Account> Accounts => Set<Account>();

    public DbSet<AccountType> AccountTypes => Set<AccountType>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
        optionsBuilder.AddInterceptors(_saveChangesInterceptor);
}
