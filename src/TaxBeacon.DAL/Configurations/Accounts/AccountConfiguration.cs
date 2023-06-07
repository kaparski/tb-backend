using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TaxBeacon.DAL.Configurations.Accounts;
public class AccountConfiguration: IEntityTypeConfiguration<Entities.Accounts.Account>
{
    public void Configure(EntityTypeBuilder<Entities.Accounts.Account> account)
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
            .HasColumnType("nvarchar")
            .HasMaxLength(100);

        account
            .Property(a => a.Address2)
            .HasColumnType("nvarchar")
            .HasMaxLength(100);

        account
            .Property(a => a.City)
            .HasColumnType("nvarchar")
            .HasMaxLength(100);

        account
            .Property(a => a.Country)
            .HasColumnType("nvarchar")
            .HasMaxLength(100);

        account
            .Property(a => a.County)
            .HasColumnType("nvarchar")
            .HasMaxLength(100);

        account
            .Property(a => a.DoingBusinessAs)
            .HasColumnType("nvarchar")
            .HasMaxLength(100);

        account
            .Property(a => a.Extension)
            .HasColumnType("nvarchar")
            .HasMaxLength(100);

        account
            .Property(a => a.Fax)
            .HasColumnType("nvarchar")
            .HasMaxLength(100);

        account
            .Property(a => a.LinkedInURL)
            .HasColumnType("nvarchar")
            .HasMaxLength(100);

        account
            .Property(a => a.Phone)
            .HasColumnType("nvarchar")
            .HasMaxLength(100);

        account
            .Property(a => a.State)
            .HasColumnType("nvarchar")
            .HasMaxLength(100);

        account
            .Property(a => a.Website)
            .HasColumnType("nvarchar")
            .HasMaxLength(100);
    }
}
