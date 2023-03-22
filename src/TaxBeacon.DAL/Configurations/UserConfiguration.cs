using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TaxBeacon.DAL.Configurations;

public class UserConfiguration: IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> user)
    {
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
            .Property(u => u.Status)
            .HasConversion<string>()
            .HasColumnType("nvarchar")
            .HasMaxLength(100);

        user
            .Property(u => u.FullName)
            .HasColumnType("nvarchar")
            .HasMaxLength(202)
            .HasComputedColumnSql("TRIM(CONCAT([FirstName], ' ', [LastName]))", stored: true);

        user.HasQueryFilter(b => b.IsDeleted == null || !b.IsDeleted.Value);

        user
            .HasIndex(u => u.Email)
            .IsUnique();

        user
            .Property(u => u.Id)
            .HasDefaultValueSql("NEWID()");

        user
            .Property(u => u.CreatedDateTimeUtc)
            .HasDefaultValueSql("GETUTCDATE()");
    }
}
