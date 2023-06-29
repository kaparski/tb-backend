using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaxBeacon.DAL.Administration.Entities;

namespace TaxBeacon.DAL.Administration.Configurations;

public class ServiceAreaActivityLogConfiguration: IEntityTypeConfiguration<ServiceAreaActivityLog>
{
    public void Configure(EntityTypeBuilder<ServiceAreaActivityLog> builder)
    {
        builder
            .HasOne<ServiceArea>(x => x.ServiceArea)
            .WithMany(x => x.ServiceAreaActivityLogs)
            .HasForeignKey(x => x.ServiceAreaId);

        builder
            .HasOne<Tenant>(x => x.Tenant)
            .WithMany(x => x.ServiceAreaActivityLogs)
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.ClientNoAction);

        builder
            .HasKey(x => new { x.TenantId, x.ServiceAreaId, x.Date });

        builder
            .Property(x => x.Event)
            .HasColumnType("nvarchar(max)")
            .IsRequired();

        builder
            .Property(x => x.EventType)
            .IsRequired();

        builder
            .Property(x => x.Revision)
            .IsRequired();

        builder
            .Property(x => x.TenantId)
            .IsRequired();

        builder
            .Property(x => x.ServiceAreaId)
            .IsRequired();

        builder
            .Property(x => x.Date)
            .IsRequired();
    }
}