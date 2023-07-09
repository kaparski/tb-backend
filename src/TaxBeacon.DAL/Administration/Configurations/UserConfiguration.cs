using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaxBeacon.DAL.Administration.Entities;

namespace TaxBeacon.DAL.Administration.Configurations;

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
           .Property(u => u.LegalName)
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
            .HasMaxLength(64)
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
            .IsUnique(false);

        user
            .Property(u => u.Id)
            .HasDefaultValueSql("NEWID()");

        user
            .Property(u => u.CreatedDateTimeUtc)
            .HasDefaultValueSql("GETUTCDATE()");

        user
            .Property(u => u.AadB2CObjectId)
            .HasColumnType("nvarchar")
            .HasMaxLength(256);

        user
            .HasIndex(u => u.AadB2CObjectId)
            .IsClustered(false);

        user
            .Property(u => u.UserType)
            .HasConversion<string>()
            .HasColumnType("nvarchar")
            .HasMaxLength(100);
    }
}
