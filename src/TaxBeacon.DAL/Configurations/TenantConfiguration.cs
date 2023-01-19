using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TaxBeacon.DAL.Configurations;

public class TenantConfiguration: IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> tenant) =>
        tenant
            .Property(t => t.Name)
            .HasColumnType("nvarchar")
            .HasMaxLength(100)
            .IsRequired();
}
