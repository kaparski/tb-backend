using Microsoft.EntityFrameworkCore;

namespace TaxBeacon.DAL.Interfaces;

public interface ITaxBeaconDbContext
{
    DbSet<User> Users { get; }

    DbSet<Tenant> Tenants { get; }

    DbSet<TenantUser> TenantUsers { get; }

    DbSet<Permission> Permissions { get; }

    DbSet<Role> Roles { get; }

    DbSet<TenantPermission> TenantPermissions { get; }

    DbSet<TenantRolePermission> TenantRolePermissions { get; }

    DbSet<TenantUserRole> TenantUserRoles { get; }

    DbSet<TenantRole> TenantRoles { get; }

    DbSet<UserActivityLog> UserActivityLogs { get; }

    DbSet<TableFilter> TableFilters { get; }

    DbSet<UserRole> UserRoles { get; }

    DbSet<RolePermission> RolePermissions { get; }

    public DbSet<Division> Divisions { get; }

    public DbSet<Department> Departments { get; }

    public DbSet<ServiceArea> ServiceAreas { get; }

    public DbSet<Team> Teams { get; }

    public DbSet<JobTitle> JobTitles { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
