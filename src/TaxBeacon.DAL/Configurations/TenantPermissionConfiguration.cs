using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TaxBeacon.DAL.Configurations;

public class TenantPermissionConfiguration: IEntityTypeConfiguration<TenantPermission>
{
    public void Configure(EntityTypeBuilder<TenantPermission> tenantPermission)
    {
        tenantPermission
            .HasOne<Tenant>(tp => tp.Tenant)
            .WithMany(t => t.TenantPermissions)
            .HasForeignKey(tp => tp.TenantId);

        tenantPermission
            .HasOne<Permission>(tp => tp.Permission)
            .WithMany(p => p.TenantPermissions)
            .HasForeignKey(tp => tp.PermissionId);

        tenantPermission
            .HasKey(tp => new { tp.TenantId, tp.PermissionId });
    }
}
