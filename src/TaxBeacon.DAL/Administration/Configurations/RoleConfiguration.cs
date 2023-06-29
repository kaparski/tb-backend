using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaxBeacon.Common.Enums;
using TaxBeacon.DAL.Administration.Entities;

namespace TaxBeacon.DAL.Administration.Configurations;

public class RoleConfiguration: IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> role)
    {
        role
            .Property(r => r.Name)
            .HasColumnType("nvarchar")
            .HasMaxLength(100)
            .IsRequired();

        role
            .HasIndex(r => r.Name)
            .IsUnique();

        role
            .Property(r => r.Id)
            .HasDefaultValueSql("NEWID()");

        role
            .Property(r => r.CreatedDateTimeUtc)
            .HasDefaultValueSql("GETUTCDATE()");

        role
            .Property(r => r.Type)
            .HasDefaultValue(SourceType.Tenant);
    }
}
