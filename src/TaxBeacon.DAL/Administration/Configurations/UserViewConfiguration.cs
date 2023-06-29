using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaxBeacon.DAL.Administration.Entities;

namespace TaxBeacon.DAL.Administration.Configurations;

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
