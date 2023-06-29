using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaxBeacon.DAL.Administration.Entities;

namespace TaxBeacon.DAL.Administration.Configurations;

public class PermissionConfiguration: IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> permission)
    {
        permission
            .Property(p => p.Name)
            .HasColumnType("nvarchar")
            .HasMaxLength(100)
            .IsRequired();

        permission
            .HasIndex(r => r.Name)
            .IsUnique();

        permission
            .Property(p => p.Id)
            .HasDefaultValueSql("NEWID()");

        permission
            .Property(p => p.CreatedDateTimeUtc)
            .HasDefaultValueSql("GETUTCDATE()");
    }
}
