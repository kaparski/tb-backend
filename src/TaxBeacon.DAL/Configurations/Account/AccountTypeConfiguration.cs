using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TaxBeacon.DAL.Configurations.Account;
public class AccountTypeConfiguration: IEntityTypeConfiguration<Entities.Account.AccountType>
{
    public void Configure(EntityTypeBuilder<Entities.Account.AccountType> accountType)
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
