using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaxBeacon.DAL.Administration.Entities;

namespace TaxBeacon.DAL.Administration.Configurations;

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
