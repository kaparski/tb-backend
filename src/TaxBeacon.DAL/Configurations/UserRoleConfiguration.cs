using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TaxBeacon.DAL.Configurations;

public class UserRoleConfiguration: IEntityTypeConfiguration<UserRole>
{

    public void Configure(EntityTypeBuilder<UserRole> userRole)
    {
        userRole
            .HasOne<User>(ur => ur.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(ur => ur.UserId);

        userRole
            .HasOne<Role>(ur => ur.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.RoleId);

        userRole
            .HasKey(ur => new { ur.UserId, ur.RoleId });

        userRole.HasQueryFilter(ur => ur.User.IsDeleted == null || !ur.User.IsDeleted.Value);

    }
}
