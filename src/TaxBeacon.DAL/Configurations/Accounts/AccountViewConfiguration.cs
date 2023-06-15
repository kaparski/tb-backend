using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaxBeacon.Common.Accounts;
using TaxBeacon.DAL.Entities.Accounts;

namespace TaxBeacon.DAL.Configurations.Accounts;

public class AccountViewConfiguration: IEntityTypeConfiguration<AccountView>
{
    public void Configure(EntityTypeBuilder<AccountView> accountView)
    {
        accountView
            .Property(a => a.State)
            .HasColumnType("nvarchar")
            .HasMaxLength(2)
            .HasConversion<string>();

        accountView
            .Property(a => a.ClientStatus)
            .HasConversion<string>()
            .HasColumnType("nvarchar")
            .HasMaxLength(100);

        accountView
            .Property(a => a.ClientState)
            .HasConversion(
                v => v.Name,
                v => ClientState.FromName(v, false))
            .HasColumnType("nvarchar")
            .HasMaxLength(50);

        accountView
            .Property(a => a.ReferralStatus)
            .HasConversion<string>()
            .HasColumnType("nvarchar")
            .HasMaxLength(100);

        accountView
            .Property(a => a.ReferralState)
            .HasConversion(
                v => v.Name,
                v => ReferralState.FromName(v, false))
            .HasColumnType("nvarchar")
            .HasMaxLength(50);

        accountView.ToTable("AccountsView", t => t.ExcludeFromMigrations());
    }
}
