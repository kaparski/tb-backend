using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TaxBeacon.DAL.Configurations;

public class RoleTenantPermissionConfiguration: IEntityTypeConfiguration<RoleTenantPermission>
{
    public void Configure(EntityTypeBuilder<RoleTenantPermission> roleTenantPermission)
    {
        roleTenantPermission
            .HasOne<TenantPermission>(rtp => rtp.TenantPermission)
            .WithMany(t => t.RoleTenantPermissions)
            .HasForeignKey(rtp => new { rtp.TenantId, rtp.PermissionId });

        roleTenantPermission
            .HasOne<Role>(rtp => rtp.Role)
            .WithMany(r => r.RoleTenantPermissions)
            .HasForeignKey(rtp => rtp.RoleId);

        roleTenantPermission.HasKey(rtp => new { rtp.TenantId, rtp.RoleId, rtp.PermissionId });
    }
}
