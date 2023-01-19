using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TaxBeacon.DAL.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> user)
    {
        user
            .Property(u => u.Username)
            .HasColumnType("nvarchar")
            .HasMaxLength(100)
            .IsRequired();

        user
            .Property(u => u.FirstName)
            .HasColumnType("nvarchar")
            .HasMaxLength(100)
            .IsRequired();

        user
            .Property(u => u.LastName)
            .HasColumnType("nvarchar")
            .HasMaxLength(100)
            .IsRequired();

        user
            .Property(u => u.Email)
            .HasColumnType("nvarchar")
            .HasMaxLength(150)
            .IsRequired();

        user
            .Property(u => u.UserStatus)
            .HasConversion<int>();
    }
}
