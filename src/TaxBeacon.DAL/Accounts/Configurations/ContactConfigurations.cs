using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaxBeacon.DAL.Accounts.Entities;

namespace TaxBeacon.DAL.Accounts.Configurations;
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
            .HasForeignKey(c => c.TenantId)
            .OnDelete(DeleteBehavior.NoAction);

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
            .HasMaxLength(100);

        builder
            .Property(c => c.Type)
            .HasColumnType("nvarchar")
            .HasMaxLength(20);

        builder
            .Property(c => c.FullName)
            .HasColumnType("nvarchar")
            .HasMaxLength(202)
            .HasComputedColumnSql("TRIM(CONCAT([FirstName], ' ', [LastName]))", stored: true);

        builder
            .HasIndex(c => new { c.TenantId, c.Email })
            .IsUnique();

        builder
            .Property(c => c.State)
            .HasColumnType("nvarchar")
            .HasMaxLength(2)
            .HasConversion<string>();

        builder
            .Property(c => c.City)
            .HasColumnType("nvarchar")
            .HasMaxLength(100);

        builder
            .Property(c => c.Country)
            .HasColumnType("nvarchar")
            .HasMaxLength(100);

        builder
            .Property(c => c.Zip)
            .HasColumnType("nvarchar")
            .HasMaxLength(9);

        builder
            .Property(c => c.Address)
            .HasColumnType("nvarchar")
            .HasMaxLength(200);

        builder
            .Property(c => c.Phone2)
            .HasColumnType("nvarchar")
            .HasMaxLength(15);

        builder
            .Property(c => c.Role)
            .HasColumnType("nvarchar")
            .HasMaxLength(100);

        builder
            .Property(c => c.SubRole)
            .HasColumnType("nvarchar")
            .HasMaxLength(100);
    }
}
