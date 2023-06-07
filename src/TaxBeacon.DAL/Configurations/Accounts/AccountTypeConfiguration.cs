using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TaxBeacon.DAL.Configurations.Accounts;
public class AccountTypeConfiguration: IEntityTypeConfiguration<Entities.Accounts.AccountType>
{
    public void Configure(EntityTypeBuilder<Entities.Accounts.AccountType> accountType)
    {
        accountType
            .HasKey(a => new { a.TenantId, a.AccountId, a.Type });

        accountType
            .HasOne(a => a.Tenant)
            .WithMany(t => t.AccountsTypes)
            .HasForeignKey(a => a.TenantId);

        accountType
            .HasOne(a => a.Account)
            .WithMany(t => t.AccountTypes)
            .HasForeignKey(a => a.AccountId);
    }
}
