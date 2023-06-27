using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TaxBeacon.DAL.Configurations;

public class JobTitleActivityLogConfiguration: IEntityTypeConfiguration<JobTitleActivityLog>
{
    public void Configure(EntityTypeBuilder<JobTitleActivityLog> builder)
    {
        builder
            .HasOne<JobTitle>(x => x.JobTitle)
            .WithMany(x => x.JobTitleActivityLogs)
            .HasForeignKey(x => x.JobTitleId);

        builder
            .HasOne<Tenant>(x => x.Tenant)
            .WithMany(x => x.JobTitleActivityLogs)
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.ClientNoAction);

        builder
            .HasKey(x => new { x.TenantId, x.JobTitleId, x.Date });

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
            .Property(x => x.JobTitleId)
            .IsRequired();

        builder
            .Property(x => x.Date)
            .IsRequired();
    }
}