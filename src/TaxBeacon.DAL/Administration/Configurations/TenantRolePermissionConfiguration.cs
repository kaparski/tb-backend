using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaxBeacon.DAL.Administration.Entities;

namespace TaxBeacon.DAL.Administration.Configurations;

public class TenantRolePermissionConfiguration: IEntityTypeConfiguration<TenantRolePermission>
{
    public void Configure(EntityTypeBuilder<TenantRolePermission> tenantRolePermission)
    {
        tenantRolePermission
            .HasOne<TenantPermission>(trp => trp.TenantPermission)
            .WithMany(tp => tp.TenantRolePermissions)
            .HasForeignKey(trp => new { trp.TenantId, trp.PermissionId });

        tenantRolePermission
            .HasOne<TenantRole>(trp => trp.TenantRole)
            .WithMany(tr => tr.TenantRolePermissions)
            .HasForeignKey(trp => new { trp.TenantId, trp.RoleId })
            .OnDelete(DeleteBehavior.NoAction);

        tenantRolePermission.HasKey(trp => new { trp.TenantId, trp.RoleId, trp.PermissionId });
    }
}
