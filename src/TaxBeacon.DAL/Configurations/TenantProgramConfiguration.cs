using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TaxBeacon.DAL.Configurations
{
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
        }
    }
}
