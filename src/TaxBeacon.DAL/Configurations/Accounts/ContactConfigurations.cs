using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaxBeacon.Common.Accounts;
using TaxBeacon.DAL.Entities.Accounts;

namespace TaxBeacon.DAL.Configurations.Accounts;
public class ContactConfigurations: IEntityTypeConfiguration<Contact>
{
    public void Configure(EntityTypeBuilder<Contact> builder)
    {
        builder
            .Property(c => c.Name)
            .HasColumnType("nvarchar")
            .HasMaxLength(100)
            .IsRequired();

        builder
            .Property(c => c.Id)
            .HasDefaultValueSql("NEWID()");

        builder
            .HasKey(c => c.Id)
            .IsClustered(false);

        builder
            .HasIndex(c => new { c.TenantId, c.AccountId, c.Id })
            .IsClustered();

        builder
            .HasOne(c => c.Tenant)
            .WithMany(t => t.Contacts)
            .HasForeignKey(c => c.TenantId);

        builder
            .HasOne(c => c.Account)
            .WithMany(a => a.Contacts)
            .HasForeignKey(c => c.AccountId);

        builder
            .HasIndex(c => new { c.TenantId, c.Name })
            .IsUnique();

        builder
            .Property(e => e.JobTitle)
            .HasColumnType("nvarchar")
            .HasMaxLength(100);

        builder
            .Property(e => e.Phone)
            .HasColumnType("nvarchar")
            .HasMaxLength(100);

        builder
            .Property(e => e.Status)
            .HasColumnType("nvarchar")
            .HasMaxLength(20)
            .HasConversion<string>();

        builder
            .Property(e => e.Email)
            .HasColumnType("nvarchar")
            .HasMaxLength(100)
            .HasConversion(
                v => v.Address,
                v => new System.Net.Mail.MailAddress(v));

        builder
            .Property(e => e.Type)
            .HasConversion(
                v => v.Value,
                v => ContactType.FromValue(v))
            .HasColumnType("nvarchar")
            .HasMaxLength(20);
    }
}
