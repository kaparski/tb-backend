using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaxBeacon.DAL.Accounts.Entities;

namespace TaxBeacon.DAL.Accounts.Configurations;

public class ContactActivityLogConfiguration: IEntityTypeConfiguration<ContactActivityLog>
{
    public void Configure(EntityTypeBuilder<ContactActivityLog> builder)
    {
        builder
            .HasOne(x => x.Contact)
            .WithMany(x => x.ContactActivityLogs)
            .HasForeignKey(x => new { x.TenantId, x.ContactId });

        builder
            .HasOne(x => x.Tenant)
            .WithMany(x => x.ContactActivityLogs)
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.NoAction);

        builder
            .HasKey(l => new { l.TenantId, l.ContactId, l.Date });

        builder
            .Property(l => l.Event)
            .HasColumnType("nvarchar(max)")
            .IsRequired();

        builder
            .Property(l => l.EventType)
            .IsRequired();

        builder
            .Property(l => l.Revision)
            .IsRequired();

        builder
            .Property(l => l.TenantId)
            .IsRequired();

        builder
            .Property(l => l.ContactId)
            .IsRequired();

        builder
            .Property(l => l.Date)
            .IsRequired();
    }
}
