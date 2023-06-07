using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TaxBeacon.DAL.Configurations;

public class TenantUserRoleConfiguration: IEntityTypeConfiguration<TenantUserRole>
{
    public void Configure(EntityTypeBuilder<TenantUserRole> tenantUserRole)
    {
        tenantUserRole
            .HasOne<TenantUser>(tur => tur.TenantUser)
            .WithMany(t => t.TenantUserRoles)
            .HasForeignKey(tur => new { tur.TenantId, tur.UserId });

        tenantUserRole
            .HasOne<TenantRole>(tur => tur.TenantRole)
            .WithMany(tru => tru.TenantUserRoles)
            .HasForeignKey(tur => new { tur.TenantId, tur.RoleId })
            .OnDelete(DeleteBehavior.NoAction);

        tenantUserRole.HasKey(tur => new { tur.TenantId, tur.RoleId, tur.UserId });

        tenantUserRole.HasQueryFilter(tur => tur.TenantUser.User.IsDeleted == null || !tur.TenantUser.User.IsDeleted.Value);
    }
}
