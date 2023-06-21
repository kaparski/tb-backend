using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaxBeacon.DAL.Entities.Accounts;

namespace TaxBeacon.DAL.Configurations.Accounts;
public class EntityActivityLogConfiguration: IEntityTypeConfiguration<EntityActivityLog>
{
    public void Configure(EntityTypeBuilder<EntityActivityLog> builder)
    {
        builder
            .HasOne<Entity>(x => x.Entity)
            .WithMany(x => x.EntityActivityLogs)
            .HasForeignKey(x => x.EntityId);

        builder
            .HasOne<Tenant>(x => x.Tenant)
            .WithMany(x => x.EntityActivityLogs)
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.ClientNoAction);

        builder
            .HasKey(ual => new { ual.TenantId, ual.EntityId, ual.Date });

        builder
            .Property(ual => ual.Event)
            .HasColumnType("nvarchar(max)")
            .IsRequired();

        builder
            .Property(ual => ual.EventType)
            .IsRequired();

        builder
            .Property(ual => ual.Revision)
            .IsRequired();

        builder
            .Property(ual => ual.TenantId)
            .IsRequired();

        builder
            .Property(ual => ual.EntityId)
            .IsRequired();

        builder
            .Property(ual => ual.Date)
            .IsRequired();
    }
}
