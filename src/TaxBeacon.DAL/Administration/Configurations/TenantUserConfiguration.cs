using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaxBeacon.DAL.Administration.Entities;

namespace TaxBeacon.DAL.Administration.Configurations;

public class TenantUserConfiguration: IEntityTypeConfiguration<TenantUser>
{
    public void Configure(EntityTypeBuilder<TenantUser> tenantUser)
    {
        tenantUser
            .HasOne<Tenant>(tu => tu.Tenant)
            .WithMany(t => t.TenantUsers)
            .HasForeignKey(tu => tu.TenantId);

        tenantUser
            .HasOne<User>(tu => tu.User)
            .WithMany(u => u.TenantUsers)
            .HasForeignKey(tu => tu.UserId);

        tenantUser
            .HasKey(tu => new { tu.TenantId, tu.UserId });

        tenantUser.HasQueryFilter(tu => tu.User.IsDeleted == null || !tu.User.IsDeleted.Value);
    }
}
