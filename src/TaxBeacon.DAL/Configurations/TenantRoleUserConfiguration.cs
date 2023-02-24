using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TaxBeacon.DAL.Configurations;

public class TenantRoleUserConfiguration: IEntityTypeConfiguration<TenantRoleUser>
{
    public void Configure(EntityTypeBuilder<TenantRoleUser> roleTenantUser)
    {
        roleTenantUser
            .HasOne<TenantUser>(rtu => rtu.TenantUser)
            .WithMany(t => t.TenantRoleUsers)
            .HasForeignKey(rtu => new { rtu.TenantId, rtu.UserId });

        roleTenantUser
            .HasOne<TenantRole>(rtu => rtu.TenantRole)
            .WithMany(tru => tru.TenantRoleUsers)
            .HasForeignKey(rtu => new { rtu.TenantId, rtu.RoleId })
            .OnDelete(DeleteBehavior.NoAction);

        roleTenantUser.HasKey(rtu => new { rtu.TenantId, rtu.RoleId, rtu.UserId });
    }
}
