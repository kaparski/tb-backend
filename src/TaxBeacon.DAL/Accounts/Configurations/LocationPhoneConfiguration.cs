using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaxBeacon.DAL.Accounts.Entities;
using TaxBeacon.DAL.Accounts.Extensions;
using TaxBeacon.DAL.Administration.Entities;

namespace TaxBeacon.DAL.Accounts.Configurations;

public class LocationPhoneConfiguration: IEntityTypeConfiguration<LocationPhone>
{
    public void Configure(EntityTypeBuilder<LocationPhone> builder)
    {
        builder.ConfigurePhone();

        builder
            .HasOne<Location>(p => p.Location)
            .WithMany(e => e.Phones)
            .HasForeignKey(p => new { p.TenantId, p.LocationId });

        builder
            .HasOne<Tenant>(p => p.Tenant)
            .WithMany(t => t.LocationPhones)
            .HasForeignKey(p => p.TenantId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
