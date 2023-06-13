using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
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

        accountView.ToTable("AccountsView", t => t.ExcludeFromMigrations());
    }
}
