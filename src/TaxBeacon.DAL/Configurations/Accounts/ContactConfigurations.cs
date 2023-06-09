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
            .Property(c => c.FirstName)
            .HasColumnType("nvarchar")
            .HasMaxLength(100)
            .IsRequired();

        builder
            .Property(c => c.LastName)
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
            .Property(c => c.JobTitle)
            .HasColumnType("nvarchar")
            .HasMaxLength(100);

        builder
            .Property(c => c.Phone)
            .HasColumnType("nvarchar")
            .HasMaxLength(100);

        builder
            .Property(c => c.Status)
            .HasColumnType("nvarchar")
            .HasMaxLength(20)
            .HasConversion<string>();

        builder
            .Property(c => c.Email)
            .HasColumnType("nvarchar")
            .HasMaxLength(100)
            .HasConversion(
                v => v.Address,
                v => new System.Net.Mail.MailAddress(v));

        builder
            .Property(c => c.Type)
            .HasConversion(
                v => v.Value,
                v => ContactType.FromValue(v))
            .HasColumnType("nvarchar")
            .HasMaxLength(20);

        builder
            .Property(c => c.FullName)
            .HasColumnType("nvarchar")
            .HasMaxLength(202)
            .HasComputedColumnSql("TRIM(CONCAT([FirstName], ' ', [LastName]))", stored: true);
    }
}
