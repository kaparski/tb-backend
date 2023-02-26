using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TaxBeacon.DAL.Configurations;

public class RoleConfiguration: IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> role) =>
        role
            .Property(r => r.Name)
            .HasColumnType("nvarchar")
            .HasMaxLength(100)
            .IsRequired();
}
