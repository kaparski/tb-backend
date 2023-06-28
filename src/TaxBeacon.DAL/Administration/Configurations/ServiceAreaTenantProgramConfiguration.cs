using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaxBeacon.DAL.Administration.Entities;

namespace TaxBeacon.DAL.Administration.Configurations;

public class ServiceAreaTenantProgramConfiguration: IEntityTypeConfiguration<ServiceAreaTenantProgram>
{
    public void Configure(EntityTypeBuilder<ServiceAreaTenantProgram> serviceAreaTenantProgram)
    {
        serviceAreaTenantProgram
            .HasOne<ServiceArea>(entity => entity.ServiceArea)
            .WithMany(sa => sa.ServiceAreaTenantPrograms)
            .HasForeignKey(entity => entity.ServiceAreaId);

        serviceAreaTenantProgram
            .HasOne<TenantProgram>(entity => entity.TenantProgram)
            .WithMany(tp => tp.ServiceAreaTenantPrograms)
            .HasForeignKey(entity => new { entity.TenantId, entity.ProgramId });

        serviceAreaTenantProgram
            .HasKey(entity => new { entity.TenantId, entity.ServiceAreaId, entity.ProgramId });

        serviceAreaTenantProgram
            .Property(entity => entity.IsDeleted)
            .HasDefaultValue(false);
    }
}
