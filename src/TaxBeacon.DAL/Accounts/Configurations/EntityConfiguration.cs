using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaxBeacon.Common.Accounts;
using TaxBeacon.DAL.Accounts.Entities;

namespace TaxBeacon.DAL.Accounts.Configurations;
public class EntityConfiguration: IEntityTypeConfiguration<Entity>
{
    public void Configure(EntityTypeBuilder<Entity> builder)
    {
        builder
            .Property(e => e.Name)
            .HasColumnType("nvarchar")
            .HasMaxLength(100)
            .IsRequired();

        builder
            .Property(e => e.Id)
            .HasDefaultValueSql("NEWID()");

        builder
            .HasKey(e => e.Id)
            .IsClustered(false);

        builder
            .HasIndex(e => new { e.TenantId, e.AccountId, e.Id })
            .IsClustered();

        builder
            .HasOne(e => e.Tenant)
            .WithMany(t => t.Entities)
            .HasForeignKey(e => e.TenantId)
            .OnDelete(DeleteBehavior.NoAction);

        builder
            .HasOne(e => e.Account)
            .WithMany(t => t.Entities)
            .HasForeignKey(e => e.AccountId);

        builder
            .HasIndex(e => new { e.TenantId, e.Name })
            .IsUnique();

        builder
            .Property(e => e.City)
            .HasColumnType("nvarchar")
            .HasMaxLength(100);

        builder
            .Property(e => e.StreetAddress1)
            .HasColumnType("nvarchar")
            .HasMaxLength(150)
            .IsRequired();

        builder
            .Property(e => e.StreetAddress2)
            .HasColumnType("nvarchar")
            .HasMaxLength(150);

        builder
            .Property(e => e.Address)
            .HasColumnType("nvarchar")
            .HasMaxLength(200);

        builder
            .Property(e => e.State)
            .HasColumnType("nvarchar")
            .HasMaxLength(2)
            .HasConversion<string>();

        builder
            .Property(c => c.Country)
            .HasColumnType("nvarchar")
            .HasMaxLength(100);

        builder
            .Property(e => e.Status)
            .HasColumnType("nvarchar")
            .HasMaxLength(20)
            .HasConversion<string>();

        builder
            .Property(e => e.Type)
            .HasColumnType("nvarchar")
            .HasMaxLength(20)
            .IsRequired();

        builder
            .Property(e => e.TaxYearEndType)
            .HasConversion(
                v => v.Name,
                v => TaxYearEndType.FromName(v, false))
            .HasColumnType("nvarchar")
            .HasMaxLength(20);

        builder
            .Property(e => e.Phone)
            .HasColumnType("nvarchar")
            .HasMaxLength(20);

        builder
            .Property(e => e.Fax)
            .HasColumnType("nvarchar")
            .HasMaxLength(20);

        builder
            .Property(e => e.Extension)
            .HasColumnType("nvarchar")
            .HasMaxLength(20);

        builder
           .Property(e => e.Dba)
           .HasColumnType("nvarchar")
           .HasMaxLength(20);

        builder
           .Property(e => e.Zip)
           .HasColumnType("nvarchar")
           .HasMaxLength(15)
           .IsRequired();

        builder
            .HasMany(e => e.StateIds)
            .WithOne(si => si.Entity)
            .HasForeignKey(si => si.EntityId)
            .IsRequired(false);

        builder
            .HasIndex(e => new { e.TenantId, e.Fein })
            .IsUnique();
    }
}
