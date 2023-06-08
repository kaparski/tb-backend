using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TaxBeacon.DAL.Configurations;

public class UserViewConfiguration: IEntityTypeConfiguration<UserView>
{
    public void Configure(EntityTypeBuilder<UserView> user)
    {
        user
            .Property(u => u.Status)
            .HasConversion<string>();

        user.ToTable("UsersView", t => t.ExcludeFromMigrations());
    }
}
