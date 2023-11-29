using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaxBeacon.DAL.Accounts.Entities;
using TaxBeacon.DAL.Administration.Entities;

namespace TaxBeacon.DAL.Accounts.Configurations;

public class LocationActivityLogConfiguration: IEntityTypeConfiguration<LocationActivityLog>
{
    public void Configure(EntityTypeBuilder<LocationActivityLog> locationActivityLog)
    {
        locationActivityLog
            .HasOne<Tenant>(log => log.Tenant)
            .WithMany(t => t.LocationActivityLogs)
            .HasForeignKey(log => log.TenantId)
            .OnDelete(DeleteBehavior.NoAction);

        locationActivityLog
            .HasOne<Location>(log => log.Location)
            .WithMany(l => l.LocationActivityLogs)
            .HasForeignKey(log => new { log.TenantId, log.LocationId });

        locationActivityLog.HasKey(log => new { log.TenantId, log.LocationId, log.Date });

        locationActivityLog
            .Property(log => log.TenantId)
            .IsRequired();

        locationActivityLog
            .Property(log => log.LocationId)
            .IsRequired();

        locationActivityLog
            .Property(log => log.Revision)
            .IsRequired();

        locationActivityLog
            .Property(log => log.Date)
            .IsRequired();

        locationActivityLog
            .Property(log => log.EventType)
            .IsRequired();

        locationActivityLog
            .Property(log => log.Event)
            .HasColumnType("nvarchar(max)")
            .IsRequired();
    }
}
