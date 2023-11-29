using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaxBeacon.DAL.Accounts.Entities;

namespace TaxBeacon.DAL.Accounts.Configurations;

public class LocationConfiguration: IEntityTypeConfiguration<Location>
{
    public void Configure(EntityTypeBuilder<Location> builder)
    {
        builder
            .Property(l => l.Id)
            .HasDefaultValueSql("NEWID()");

        builder
            .HasKey(l => new { l.TenantId, l.Id })
            .IsClustered(false);

        builder
            .Property(l => l.Name)
            .HasColumnType("nvarchar")
            .HasMaxLength(100)
            .IsRequired();

        builder
            .Property(c => c.Country)
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
            .Property(l => l.City)
            .HasColumnType("nvarchar")
            .HasMaxLength(100);

        builder
            .Property(l => l.County)
            .HasColumnType("nvarchar")
            .HasMaxLength(150);

        builder
            .Property(l => l.State)
            .HasColumnType("nvarchar")
            .HasMaxLength(2)
            .HasConversion<string>();

        builder
            .Property(a => a.Zip)
            .HasColumnType("nvarchar")
            .HasMaxLength(10);

        builder
            .Property(a => a.Address)
            .HasColumnType("nvarchar")
            .HasMaxLength(200);

        builder
            .Property(e => e.Type)
            .HasConversion<string>()
            .HasColumnType("nvarchar")
            .HasMaxLength(20);

        builder
            .Property(l => l.LocationId)
            .HasColumnType("nvarchar")
            .HasMaxLength(100)
            .IsRequired();

        builder
            .Property(l => l.Status)
            .HasColumnType("nvarchar")
            .HasMaxLength(20)
            .HasConversion<string>();

        builder
            .HasIndex(l => new { l.TenantId, l.AccountId, l.Id })
            .IsClustered();

        builder
            .HasOne(l => l.Tenant)
            .WithMany(t => t.Locations)
            .HasForeignKey(l => l.TenantId)
            .OnDelete(DeleteBehavior.NoAction);

        builder
            .HasOne(l => l.Account)
            .WithMany(t => t.Locations)
            .HasForeignKey(l => new { l.TenantId, l.AccountId });

        builder
            .HasOne(a => a.NaicsCode)
            .WithMany(nc => nc.Locations)
            .HasForeignKey(a => a.PrimaryNaicsCode);

        builder
            .HasIndex(l => new { l.AccountId, l.Name })
            .IsUnique();

        builder
            .HasIndex(l => new { l.TenantId, l.LocationId })
            .IsUnique();
    }
}
