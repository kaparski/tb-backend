using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaxBeacon.DAL.Accounts.Entities;
using TaxBeacon.DAL.Administration.Entities;

namespace TaxBeacon.DAL.Accounts.Configurations;

public class StateIdConfiguration: IEntityTypeConfiguration<StateId>
{
    public void Configure(EntityTypeBuilder<StateId> builder)
    {
        builder.HasKey(x => new { x.TenantId, x.Id }).IsClustered(false);
        builder.HasIndex(x => new { x.TenantId, x.EntityId, x.Id }).IsClustered();
        builder.HasIndex(x => new { x.TenantId, x.StateIdCode }).IsUnique();
        builder.HasIndex(x => new { x.EntityId, x.State }).IsUnique();

        builder.Property(x => x.Id).HasDefaultValueSql("NEWID()");

        builder
            .Property(e => e.State)
            .IsRequired()
            .HasColumnType("nvarchar")
            .HasMaxLength(2)
            .HasConversion<string>();

        builder
            .Property(e => e.StateIdType)
            .IsRequired()
            .HasColumnType("nvarchar")
            .HasMaxLength(25);

        builder
            .Property(e => e.StateIdCode)
            .IsRequired()
            .HasMaxLength(25);

        builder.Property(e => e.LocalJurisdiction)
            .HasMaxLength(100);

        builder
            .Property(e => e.CreatedDateTimeUtc)
            .HasDefaultValueSql("GETUTCDATE()");

        builder
            .HasOne<Tenant>(s => s.Tenant)
            .WithMany(t => t.StateIds)
            .HasForeignKey(s => s.TenantId)
            .OnDelete(DeleteBehavior.NoAction);

        builder
            .HasOne<Entity>(s => s.Entity)
            .WithMany(e => e.StateIds)
            .HasForeignKey(s => new { s.TenantId, s.EntityId });
    }
}
