using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TaxBeacon.DAL.Configurations;

public class RolePermissionConfiguration: IEntityTypeConfiguration<RolePermission>
{

    public void Configure(EntityTypeBuilder<RolePermission> rolePermission)
    {
        rolePermission
            .HasOne<Role>(rp => rp.Role)
            .WithMany(r => r.RolePermissions)
            .HasForeignKey(rp => rp.RoleId);

        rolePermission
            .HasOne<Permission>(rp => rp.Permission)
            .WithMany(p => p.RolePermissions)
            .HasForeignKey(rp => rp.PermissionId);

        rolePermission
            .HasKey(rp => new { rp.RoleId, rp.PermissionId });
    }
}
