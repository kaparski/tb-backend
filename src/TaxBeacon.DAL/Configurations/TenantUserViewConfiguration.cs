using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TaxBeacon.DAL.Configurations;

public class TenantUserViewConfiguration: IEntityTypeConfiguration<TenantUserView>
{
    public void Configure(EntityTypeBuilder<TenantUserView> user)
    {
        user
            .Property(u => u.Status)
            .HasConversion<string>();

        user.ToTable("TenantUsersView", t => t.ExcludeFromMigrations());
    }
}
