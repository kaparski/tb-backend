using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaxBeacon.DAL.Accounts.Common;

namespace TaxBeacon.DAL.Accounts.Extensions;

public static class PhoneBaseConfigurationExtension
{
    public static EntityTypeBuilder<T> ConfigurePhone<T>(this EntityTypeBuilder<T> builder) where T : PhoneBase
    {
        builder
            .Property(t => t.Id)
            .HasDefaultValueSql("NEWID()");

        builder
            .Property(a => a.Number)
            .HasColumnType("nvarchar")
            .HasMaxLength(15)
            .IsRequired();

        builder
            .Property(a => a.Type)
            .HasColumnType("nvarchar")
            .HasMaxLength(10)
            .HasConversion<string>()
            .IsRequired();

        builder
            .Property(a => a.Extension)
            .HasColumnType("nvarchar")
            .HasMaxLength(200);

        builder
            .HasKey(p => new { p.TenantId, p.Id });

        return builder;
    }
}
