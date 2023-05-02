using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaxBeacon.DAL.Entities;

namespace TaxBeacon.DAL.Configurations;

public class TenantUserRoleConfiguration: IEntityTypeConfiguration<TenantUserRole>
{
    public void Configure(EntityTypeBuilder<TenantUserRole> roleTenantUser)
    {
        roleTenantUser
            .HasOne<TenantUser>(tur => tur.TenantUser)
            .WithMany(t => t.TenantUserRoles)
            .HasForeignKey(tur => new { tur.TenantId, tur.UserId });

        roleTenantUser
            .HasOne<TenantRole>(tur => tur.TenantRole)
            .WithMany(tru => tru.TenantUserRoles)
            .HasForeignKey(tur => new { tur.TenantId, tur.RoleId })
            .OnDelete(DeleteBehavior.NoAction);

        roleTenantUser.HasKey(tur => new { tur.TenantId, tur.RoleId, tur.UserId });

        roleTenantUser.HasQueryFilter(tur => tur.TenantUser.User.IsDeleted == null || !tur.TenantUser.User.IsDeleted.Value);

    }
}
