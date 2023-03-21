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

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
