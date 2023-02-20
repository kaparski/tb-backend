using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TaxBeacon.DAL.Configurations;

public class RoleTenantUserConfiguration: IEntityTypeConfiguration<RoleTenantUser>
{
    public void Configure(EntityTypeBuilder<RoleTenantUser> roleTenantUser)
    {
        roleTenantUser
            .HasOne<TenantUser>(rtu => rtu.TenantUser)
            .WithMany(t => t.RoleTenantUsers)
            .HasForeignKey(rtu => new { rtu.TenantId, rtu.UserId });

        roleTenantUser
            .HasOne<Role>(rtu => rtu.Role)
            .WithMany(r => r.RoleTenantUsers)
            .HasForeignKey(rtu => rtu.RoleId);

        roleTenantUser.HasKey(rtu => new { rtu.TenantId, rtu.RoleId, rtu.UserId });
    }
}
