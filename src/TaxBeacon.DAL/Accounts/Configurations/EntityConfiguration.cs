using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaxBeacon.Common.Enums.Accounts;
using TaxBeacon.DAL.Accounts.Entities;
using TaxBeacon.DAL.Administration.Entities;

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
            .Property(e => e.Status)
            .HasColumnType("nvarchar")
            .HasMaxLength(20)
            .HasConversion<string>();

        builder
            .Property(a => a.DoingBusinessAs)
            .HasColumnType("nvarchar")
            .HasMaxLength(100);

        builder
            .Property(a => a.Country)
            .HasColumnType("nvarchar")
            .HasMaxLength(100)
            .IsRequired();

        builder
            .Property(a => a.Address1)
            .HasColumnType("nvarchar")
            .HasMaxLength(100);

        builder
            .Property(a => a.Address2)
            .HasColumnType("nvarchar")
            .HasMaxLength(100);

        builder
            .Property(a => a.County)
            .HasColumnType("nvarchar")
            .HasMaxLength(150);

        builder
            .Property(a => a.State)
            .HasColumnType("nvarchar")
            .HasMaxLength(2)
            .HasConversion<string>();

        builder
            .Property(a => a.City)
            .HasColumnType("nvarchar")
            .HasMaxLength(100);

        builder
            .Property(a => a.Address)
            .HasColumnType("nvarchar")
            .HasMaxLength(200);

        builder
            .Property(a => a.Zip)
            .HasColumnType("nvarchar")
            .HasMaxLength(10);

        builder
            .Property(l => l.EntityId)
            .HasColumnType("nvarchar")
            .HasMaxLength(100)
            .IsRequired();

        builder
            .Property(e => e.Type)
            .HasColumnType("nvarchar")
            .HasMaxLength(20)
            .IsRequired();

        builder
            .Property(e => e.TaxYearEndType)
            .HasColumnType("nvarchar")
            .HasMaxLength(20);

        builder
            .Property(e => e.Fein)
            .HasColumnType("nvarchar")
            .HasMaxLength(9);

        builder
            .Property(e => e.Ein)
            .HasColumnType("nvarchar")
            .HasMaxLength(20);

        builder
            .Property(e => e.JurisdictionId)
            .HasColumnType("nvarchar")
            .HasMaxLength(20);

        builder
            .HasIndex(e => new { e.TenantId, e.Fein })
            .IsUnique();

        builder
            .HasIndex(e => new { e.TenantId, e.Ein })
            .IsUnique();

        builder
            .HasKey(e => new { e.TenantId, e.Id });

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
            .HasForeignKey(e => new { e.TenantId, e.AccountId });

        builder
            .HasIndex(e => new { e.TenantId, e.Name })
            .IsUnique();

        builder
            .HasOne(a => a.NaicsCode)
            .WithMany(nc => nc.Entities)
            .HasForeignKey(a => a.PrimaryNaicsCode);

        builder
            .HasIndex(l => new { l.TenantId, l.EntityId })
            .IsUnique();
    }
}
