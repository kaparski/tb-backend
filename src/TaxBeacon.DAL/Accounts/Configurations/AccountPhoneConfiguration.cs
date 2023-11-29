using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaxBeacon.DAL.Accounts.Entities;
using TaxBeacon.DAL.Accounts.Extensions;
using TaxBeacon.DAL.Administration.Entities;

namespace TaxBeacon.DAL.Accounts.Configurations;

public class AccountPhoneConfiguration: IEntityTypeConfiguration<AccountPhone>
{
    public void Configure(EntityTypeBuilder<AccountPhone> builder)
    {
        builder.ConfigurePhone();

        builder
            .HasOne<Account>(p => p.Account)
            .WithMany(e => e.Phones)
            .HasForeignKey(p => new { p.TenantId, p.AccountId });

        builder
            .HasOne<Tenant>(p => p.Tenant)
            .WithMany(t => t.AccountPhones)
            .HasForeignKey(p => p.TenantId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
