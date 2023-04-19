using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TaxBeacon.DAL.Configurations;

public class TenantActivityLogConfiguration: IEntityTypeConfiguration<TenantActivityLog>
{
    public void Configure(EntityTypeBuilder<TenantActivityLog> builder)
    {
        builder
            .HasOne<Tenant>(tal => tal.Tenant)
            .WithMany(t => t.TenantActivityLogs)
            .HasForeignKey(tal => tal.TenantId);

        builder
            .HasKey(ual => new { ual.TenantId, ual.Date });

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
            .Property(ual => ual.Date)
            .IsRequired();
    }
}
