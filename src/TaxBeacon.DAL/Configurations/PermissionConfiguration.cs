using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TaxBeacon.DAL.Configurations;

public class PermissionConfiguration: IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> permission)
    {
        permission.HasKey(p => p.Id);

        permission
            .Property(p => p.Name)
            .HasColumnType("nvarchar")
            .HasMaxLength(100)
            .IsRequired();

        var permissions =
            Enum.GetValues<TaxBeacon.Common.Enums.Permission>()
                .Select(p => new Permission { Id = (int)p, Name = p.ToString() });

        permission.HasData(permissions);
    }
}
