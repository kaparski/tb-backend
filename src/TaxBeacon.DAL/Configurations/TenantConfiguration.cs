﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaxBeacon.DAL.Entities;

namespace TaxBeacon.DAL.Configurations;

public class TenantConfiguration: IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> tenant)
    {
        tenant
            .Property(t => t.Name)
            .HasColumnType("nvarchar")
            .HasMaxLength(100)
            .IsRequired();

        tenant
            .HasIndex(t => t.Name)
            .IsUnique();

        tenant
            .Property(t => t.Id)
            .HasDefaultValueSql("NEWID()");

        tenant
            .Property(t => t.CreatedDateTimeUtc)
            .HasDefaultValueSql("GETUTCDATE()");

        tenant
            .Property(u => u.Status)
            .HasConversion<string>()
            .HasColumnType("nvarchar")
            .HasMaxLength(100);

        tenant
            .Property(t => t.DivisionEnabled)
            .HasDefaultValue(true);
    }
}
