using Microsoft.EntityFrameworkCore;
using System.Reflection;
using TaxBeacon.DAL.Accounts;
using TaxBeacon.DAL.Accounts.Entities;
using TaxBeacon.DAL.Administration;
using TaxBeacon.DAL.Administration.Entities;
using TaxBeacon.DAL.Interceptors;

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

    public DbSet<TenantUserView> TenantUsersView => Set<TenantUserView>();

    public DbSet<Account> Accounts => Set<Account>();

    public DbSet<Client> Clients => Set<Client>();

    public DbSet<Referral> Referrals => Set<Referral>();

    public DbSet<DepartmentTenantProgram> DepartmentTenantPrograms => Set<DepartmentTenantProgram>();

    public DbSet<ServiceAreaTenantProgram> ServiceAreaTenantPrograms => Set<ServiceAreaTenantProgram>();

    public DbSet<Entity> Entities => Set<Entity>();

    public DbSet<Location> Locations => Set<Location>();

    public DbSet<Contact> Contacts => Set<Contact>();

    public DbSet<AccountView> AccountsView => Set<AccountView>();

    public DbSet<StateId> StateIds => Set<StateId>();

    public DbSet<ClientManager> ClientManagers => Set<ClientManager>();

    public DbSet<EntityActivityLog> EntityActivityLogs => Set<EntityActivityLog>();

    public DbSet<ContactActivityLog> ContactActivityLogs => Set<ContactActivityLog>();

    public DbSet<AccountActivityLog> AccountActivityLogs => Set<AccountActivityLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
        optionsBuilder.AddInterceptors(_saveChangesInterceptor);
}
