using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaxBeacon.DAL.Accounts.Entities;
using TaxBeacon.DAL.Administration.Entities;

namespace TaxBeacon.DAL.Accounts.Configurations;

public class AccountContactActivityLogConfiguration: IEntityTypeConfiguration<AccountContactActivityLog>
{
    public void Configure(EntityTypeBuilder<AccountContactActivityLog> builder)
    {
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
            .Property(x => x.ContactId)
            .IsRequired();

        builder
            .Property(x => x.AccountId)
            .IsRequired();

        builder
            .Property(x => x.Date)
            .IsRequired();

        builder.HasKey(x => new { x.TenantId, x.AccountId, x.ContactId, x.Date });

        builder
            .HasOne<AccountContact>(ac => ac.AccountContact)
            .WithMany(a => a.AccountContactActivityLogs)
            .HasForeignKey(x => new { x.TenantId, x.AccountId, x.ContactId });

        builder
            .HasOne<Tenant>(x => x.Tenant)
            .WithMany(t => t.AccountContactActivityLogs)
            .HasForeignKey(acl => acl.TenantId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
