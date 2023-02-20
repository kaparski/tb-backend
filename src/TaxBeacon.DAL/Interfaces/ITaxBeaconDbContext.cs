using Microsoft.EntityFrameworkCore;

namespace TaxBeacon.DAL.Interfaces;

public interface ITaxBeaconDbContext
{
    DbSet<User> Users { get; }

    DbSet<Tenant> Tenants { get; }

    DbSet<TenantUser> TenantUsers { get; }

    DbSet<Permission> Permissions { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
