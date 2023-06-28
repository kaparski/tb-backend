using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaxBeacon.DAL.Administration.Entities;

namespace TaxBeacon.DAL.Administration.Configurations;

public class TenantRoleConfiguration: IEntityTypeConfiguration<TenantRole>
{
    public void Configure(EntityTypeBuilder<TenantRole> tenantRole)
    {
        tenantRole
            .HasOne<Tenant>(tr => tr.Tenant)
            .WithMany(t => t.TenantRoles)
            .HasForeignKey(tu => tu.TenantId);

        tenantRole
            .HasOne<Role>(tr => tr.Role)
            .WithMany(r => r.TenantRoles)
            .HasForeignKey(tr => tr.RoleId);

        tenantRole
            .HasKey(tr => new { tr.TenantId, tr.RoleId });
    }
}
