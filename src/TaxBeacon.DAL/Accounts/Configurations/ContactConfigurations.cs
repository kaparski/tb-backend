using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaxBeacon.DAL.Accounts.Entities;

namespace TaxBeacon.DAL.Accounts.Configurations;

public class ContactConfigurations: IEntityTypeConfiguration<Contact>
{
    public void Configure(EntityTypeBuilder<Contact> contact)
    {
        contact
            .HasKey(c => new { c.TenantId, c.Id });

        contact
            .Property(c => c.Id)
            .HasDefaultValueSql("NEWID()");

        contact
            .Property(c => c.Email)
            .HasColumnType("nvarchar")
            .HasMaxLength(64)
            .IsRequired();

        contact
            .Property(c => c.SecondaryEmail)
            .HasColumnType("nvarchar")
            .HasMaxLength(64);

        contact
            .Property(c => c.FirstName)
            .HasColumnType("nvarchar")
            .HasMaxLength(100)
            .IsRequired();

        contact
            .Property(c => c.LastName)
            .HasColumnType("nvarchar")
            .HasMaxLength(100)
            .IsRequired();

        contact
            .Property(c => c.JobTitle)
            .HasColumnType("nvarchar")
            .HasMaxLength(100);

        contact
            .Property(c => c.FullName)
            .HasColumnType("nvarchar")
            .HasMaxLength(202)
            .HasComputedColumnSql("TRIM(CONCAT([FirstName], ' ', [LastName]))", stored: true);

        contact
            .Property(c => c.CreatedDateTimeUtc)
            .HasDefaultValueSql("GETUTCDATE()");

        contact
            .HasOne(c => c.Tenant)
            .WithMany(t => t.Contacts)
            .HasForeignKey(c => c.TenantId)
            .OnDelete(DeleteBehavior.NoAction);

        contact
            .HasIndex(c => new { c.TenantId, c.Email })
            .IsUnique();
    }
}
