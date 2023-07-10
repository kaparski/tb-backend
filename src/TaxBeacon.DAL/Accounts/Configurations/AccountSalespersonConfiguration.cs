using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaxBeacon.DAL.Accounts.Entities;
using TaxBeacon.DAL.Administration.Entities;

namespace TaxBeacon.DAL.Accounts.Configurations;

public class AccountSalespersonConfiguration: IEntityTypeConfiguration<AccountSalesperson>
{
    public void Configure(EntityTypeBuilder<AccountSalesperson> tenantUserAccount)
    {
        tenantUserAccount
            .HasOne<TenantUser>(tua => tua.TenantUser)
            .WithMany(tu => tu.TenantUserAccounts)
            .HasForeignKey(tua => new { tua.TenantId, tua.UserId })
            .OnDelete(DeleteBehavior.NoAction);

        tenantUserAccount
            .HasOne<Account>(tua => tua.Account)
            .WithMany(a => a.Salespersons)
            .HasForeignKey(tua => tua.AccountId);

        tenantUserAccount.HasKey(tua => new { tua.TenantId, tua.AccountId, tua.UserId });
    }
}
