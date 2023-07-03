using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaxBeacon.DAL.Accounts.Entities;
using TaxBeacon.DAL.Administration.Entities;

namespace TaxBeacon.DAL.Accounts.Configurations;

public class AccountActivityLogConfiguration: IEntityTypeConfiguration<AccountActivityLog>
{
    public void Configure(EntityTypeBuilder<AccountActivityLog> accountActivityLog)
    {
        accountActivityLog
            .HasOne<Account>(acl => acl.Account)
            .WithMany(a => a.AccountActivityLogs)
            .HasForeignKey(acl => acl.AccountId);

        accountActivityLog
            .HasOne<Tenant>(acl => acl.Tenant)
            .WithMany(t => t.AccountActivityLogs)
            .HasForeignKey(acl => acl.TenantId);

        accountActivityLog.HasKey(acl => new { acl.TenantId, acl.AccountId, acl.Date });

        accountActivityLog
            .Property(acl => acl.Event)
            .HasColumnType("nvarchar(max)")
            .IsRequired();

        accountActivityLog
            .Property(acl => acl.EventType)
            .IsRequired();

        accountActivityLog
            .Property(acl => acl.AccountPartType)
            .IsRequired();

        accountActivityLog
            .Property(acl => acl.Revision)
            .IsRequired();

        accountActivityLog
            .Property(acl => acl.TenantId)
            .IsRequired();

        accountActivityLog
            .Property(acl => acl.AccountId)
            .IsRequired();

        accountActivityLog
            .Property(acl => acl.Date)
            .IsRequired();
    }
}
