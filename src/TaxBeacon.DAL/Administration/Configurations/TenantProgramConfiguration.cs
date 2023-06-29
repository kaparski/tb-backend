using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaxBeacon.DAL.Administration.Entities;

namespace TaxBeacon.DAL.Administration.Configurations;

public class TenantProgramConfiguration: IEntityTypeConfiguration<TenantProgram>
{
    public void Configure(EntityTypeBuilder<TenantProgram> tenantProgram)
    {
        tenantProgram
            .HasOne(tp => tp.Tenant)
            .WithMany(t => t.TenantsPrograms)
            .HasForeignKey(ur => ur.TenantId);

        tenantProgram
            .HasOne(ur => ur.Program)
            .WithMany(r => r.TenantsPrograms)
            .HasForeignKey(ur => ur.ProgramId);

        tenantProgram
            .HasKey(tp => new { tp.TenantId, tp.ProgramId })
            .IsClustered(false);

        tenantProgram
            .HasIndex(tp => new { tp.TenantId, tp.Status, tp.ProgramId })
            .IsUnique()
            .IsClustered();

        tenantProgram
            .Property(tp => tp.IsDeleted)
            .HasDefaultValue(false);
    }
}