using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaxBeacon.DAL.Accounts.Entities;
using TaxBeacon.DAL.Accounts.Extensions;
using TaxBeacon.DAL.Administration.Entities;

namespace TaxBeacon.DAL.Accounts.Configurations;

public class EntityPhoneConfiguration: IEntityTypeConfiguration<EntityPhone>
{
    public void Configure(EntityTypeBuilder<EntityPhone> builder)
    {
        builder.ConfigurePhone();

        builder
            .HasOne<Entity>(p => p.Entity)
            .WithMany(e => e.Phones)
            .HasForeignKey(p => new { p.TenantId, p.EntityId });

        builder
            .HasOne<Tenant>(p => p.Tenant)
            .WithMany(t => t.EntitiesPhones)
            .HasForeignKey(p => p.TenantId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
