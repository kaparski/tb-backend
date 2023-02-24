using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TaxBeacon.DAL.Configurations;

public class TenantRoleConfiguration: IEntityTypeConfiguration<TenantRole>
{
    public void Configure(EntityTypeBuilder<TenantRole> tenantRole)
    {
        tenantRole
            .HasOne<Role>(tr => tr.Role)
            .WithMany(r => r.TenantRoles)
            .HasForeignKey(tr => tr.RoleId);

        tenantRole
            .HasOne<Tenant>(tr => tr.Tenant)
            .WithMany(t => t.TenantRoles)
            .HasForeignKey(tr => tr.TenantId);

        tenantRole.HasKey(tr => new { tr.TenantId, tr.RoleId });
    }
}
