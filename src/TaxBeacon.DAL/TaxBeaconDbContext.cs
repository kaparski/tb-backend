using Microsoft.EntityFrameworkCore;
using System.Reflection;
using TaxBeacon.DAL.Interceptors;
using TaxBeacon.DAL.Interfaces;

namespace TaxBeacon.DAL;

public class TaxBeaconDbContext: DbContext, ITaxBeaconDbContext
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
        optionsBuilder.AddInterceptors(_saveChangesInterceptor);
}
