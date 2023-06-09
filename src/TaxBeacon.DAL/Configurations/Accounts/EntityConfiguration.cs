using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaxBeacon.Common.Accounts;
using TaxBeacon.DAL.Entities.Accounts;

namespace TaxBeacon.DAL.Configurations.Accounts;
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
            .Property(e => e.State)
            .HasColumnType("nvarchar")
            .HasMaxLength(2)
            .HasConversion<string>();

        builder
            .Property(e => e.Status)
            .HasColumnType("nvarchar")
            .HasMaxLength(20)
            .HasConversion<string>();

        builder
            .Property(e => e.Type)
            .HasConversion(
                v => v.Value,
                v => EntityType.FromValue(v))
            .HasColumnType("nvarchar")
            .HasMaxLength(20);
    }
}
