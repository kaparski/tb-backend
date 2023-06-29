using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaxBeacon.DAL.Accounts.Entities;

namespace TaxBeacon.DAL.Accounts.Configurations;

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
            .HasColumnType("nvarchar")
            .HasMaxLength(50);

        accountView
            .Property(a => a.ReferralStatus)
            .HasConversion<string>()
            .HasColumnType("nvarchar")
            .HasMaxLength(100);

        accountView
            .Property(a => a.ReferralState)
            .HasColumnType("nvarchar")
            .HasMaxLength(50);

        accountView.ToTable("AccountsView", t => t.ExcludeFromMigrations());
    }
}
