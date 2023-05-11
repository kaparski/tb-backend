using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TaxBeacon.DAL.Configurations;

public class TenantUserRoleConfigurations: IEntityTypeConfiguration<TenantUserRole>
{
    public void Configure(EntityTypeBuilder<TenantUserRole> tenantUserRole)
    {
        tenantUserRole
            .HasOne<TenantUser>(tur => tur.TenantUser)
            .WithMany(t => t.TenantUserRoles)
            .HasForeignKey(tur => new { tur.TenantId, tur.UserId });

        tenantUserRole
            .HasOne<TenantRole>(tur => tur.TenantRole)
            .WithMany(r => r.TenantUserRoles)
            .HasForeignKey(tur => new { tur.TenantId, tur.RoleId })
            .OnDelete(DeleteBehavior.NoAction);

        tenantUserRole.HasKey(tur => new { tur.TenantId, tur.RoleId, tur.UserId });
    }
}
