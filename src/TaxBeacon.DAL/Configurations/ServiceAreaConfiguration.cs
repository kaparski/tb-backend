using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TaxBeacon.DAL.Configurations;

public class ServiceAreaConfiguration: IEntityTypeConfiguration<ServiceArea>
{
    public void Configure(EntityTypeBuilder<ServiceArea> serviceArea)
    {
        serviceArea
            .Property(sa => sa.Name)
            .HasColumnType("nvarchar")
            .HasMaxLength(100)
            .IsRequired();

        serviceArea
            .Property(sa => sa.Description)
            .HasColumnType("nvarchar")
            .HasMaxLength(200)
            .IsRequired(false);

        serviceArea
            .HasKey(sa => sa.Id)
            .IsClustered(false);

        serviceArea
            .HasIndex(sa => new { sa.TenantId, sa.DepartmentId, sa.Id })
            .IsClustered();

        serviceArea
            .HasIndex(sa => new { sa.TenantId, sa.Name })
            .IsUnique();

        serviceArea
            .HasOne(sa => sa.Tenant)
            .WithMany(t => t.ServiceAreas)
            .HasForeignKey(sa => sa.TenantId)
            .OnDelete(DeleteBehavior.NoAction);

        serviceArea
            .HasMany(sa => sa.Users)
            .WithOne(u => u.ServiceArea)
            .HasForeignKey(u => u.ServiceAreaId);
    }
}