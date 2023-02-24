using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TaxBeacon.DAL.Configurations;

public class TenantRolePermissionConfiguration: IEntityTypeConfiguration<TenantRolePermission>
{
    public void Configure(EntityTypeBuilder<TenantRolePermission> roleTenantPermission)
    {
        roleTenantPermission
            .HasOne<TenantPermission>(trp => trp.TenantPermission)
            .WithMany(trp => trp.TenantRolePermissions)
            .HasForeignKey(trp => new { trp.TenantId, trp.PermissionId });

        roleTenantPermission
            .HasOne<TenantRole>(trp => trp.TenantRole)
            .WithMany(tr => tr.TenantRolePermissions)
            .HasForeignKey(trp => new { trp.TenantId, trp.RoleId })
            .OnDelete(DeleteBehavior.NoAction);

        roleTenantPermission.HasKey(trp => new { trp.TenantId, trp.RoleId, trp.PermissionId });
    }
}
