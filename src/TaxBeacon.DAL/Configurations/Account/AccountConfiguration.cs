using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TaxBeacon.DAL.Configurations.Account;
public class AccountConfiguration: IEntityTypeConfiguration<Entities.Account.Account>
{
    public void Configure(EntityTypeBuilder<Entities.Account.Account> account)
    {
        account
            .Property(a => a.Name)
            .HasColumnType("nvarchar")
            .HasMaxLength(100)
            .IsRequired();

        account
            .Property(t => t.Id)
            .HasDefaultValueSql("NEWID()");

        account
            .HasKey(a => a.Id)
            .IsClustered(false);

        account
            .HasIndex(a => new { a.TenantId, a.Id })
            .IsClustered();

        account
            .HasOne(a => a.Tenant)
            .WithMany(t => t.Accounts)
            .HasForeignKey(a => a.TenantId);

        account
            .HasIndex(d => new { d.TenantId, d.Name })
            .IsUnique();

        account
            .Property(a => a.Address1)
            .HasColumnType("ncarchar")
            .HasMaxLength(100);

        account
            .Property(a => a.Address2)
            .HasColumnType("ncarchar")
            .HasMaxLength(100);

        account
            .Property(a => a.City)
            .HasColumnType("ncarchar")
            .HasMaxLength(100);

        account
            .Property(a => a.Country)
            .HasColumnType("ncarchar")
            .HasMaxLength(100);

        account
            .Property(a => a.County)
            .HasColumnType("ncarchar")
            .HasMaxLength(100);

        account
            .Property(a => a.DoingBusinessAs)
            .HasColumnType("ncarchar")
            .HasMaxLength(100);

        account
            .Property(a => a.Extension)
            .HasColumnType("ncarchar")
            .HasMaxLength(100);

        account
            .Property(a => a.Fax)
            .HasColumnType("ncarchar")
            .HasMaxLength(100);

        account
            .Property(a => a.LinkedInURL)
            .HasColumnType("ncarchar")
            .HasMaxLength(100);

        account
            .Property(a => a.Phone)
            .HasColumnType("ncarchar")
            .HasMaxLength(100);

        account
            .Property(a => a.State)
            .HasColumnType("ncarchar")
            .HasMaxLength(100);

        account
            .Property(a => a.Website)
            .HasColumnType("ncarchar")
            .HasMaxLength(100);
    }
}
