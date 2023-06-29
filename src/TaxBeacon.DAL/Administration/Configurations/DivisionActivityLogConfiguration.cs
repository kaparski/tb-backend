using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaxBeacon.DAL.Administration.Entities;

namespace TaxBeacon.DAL.Administration.Configurations;

public class DivisionActivityLogConfiguration: IEntityTypeConfiguration<DivisionActivityLog>
{
    public void Configure(EntityTypeBuilder<DivisionActivityLog> builder)
    {
        builder
            .HasOne<Division>(x => x.Division)
            .WithMany(x => x.DivisionActivityLogs)
            .HasForeignKey(x => x.DivisionId);

        builder
            .HasOne<Tenant>(x => x.Tenant)
            .WithMany(x => x.DivisionActivityLogs)
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.ClientNoAction);

        builder
            .HasKey(ual => new { ual.TenantId, ual.DivisionId, ual.Date });

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
            .Property(ual => ual.DivisionId)
            .IsRequired();

        builder
            .Property(ual => ual.Date)
            .IsRequired();
    }
}