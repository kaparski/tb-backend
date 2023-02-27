using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TaxBeacon.DAL.Configurations;

public class PermissionConfiguration: IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> permission)
    {
        permission
            .Property(p => p.Name)
            .HasConversion<string>()
            .HasColumnType("nvarchar")
            .HasMaxLength(100)
            .IsRequired();

        permission
            .HasIndex(r => r.Name)
            .IsUnique();

        permission
            .Property(p => p.Id)
            .HasDefaultValueSql("NEWID()");
    }
}
