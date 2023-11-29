using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaxBeacon.DAL.Accounts.Entities;
using TaxBeacon.DAL.Administration.Entities;

namespace TaxBeacon.DAL.Accounts.Configurations;

public class EntityLocationConfiguration: IEntityTypeConfiguration<EntityLocation>
{
    public void Configure(EntityTypeBuilder<EntityLocation> entityLocation)
    {
        entityLocation
            .HasOne<Entity>(el => el.Entity)
            .WithMany(e => e.EntityLocations)
            .HasForeignKey(el => new { el.TenantId, el.EntityId });

        entityLocation
            .HasOne<Location>(el => el.Location)
            .WithMany(l => l.EntityLocations)
            .HasForeignKey(el => new { el.TenantId, el.LocationId })
            .OnDelete(DeleteBehavior.NoAction);

        entityLocation
            .HasOne<Tenant>(el => el.Tenant)
            .WithMany(t => t.EntityLocations)
            .HasForeignKey(el => el.TenantId)
            .OnDelete(DeleteBehavior.NoAction);

        entityLocation
            .HasKey(el => new { el.TenantId, el.EntityId, el.LocationId });
    }
}
