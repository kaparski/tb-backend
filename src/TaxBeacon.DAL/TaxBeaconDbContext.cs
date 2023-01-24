using Microsoft.EntityFrameworkCore;
using System.Reflection;
using TaxBeacon.Common.Enums;
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        modelBuilder.Entity<User>().HasQueryFilter(b => !b.IsDeleted.HasValue || !b.IsDeleted.Value);
        base.OnModelCreating(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
        optionsBuilder.AddInterceptors(_saveChangesInterceptor);
}
