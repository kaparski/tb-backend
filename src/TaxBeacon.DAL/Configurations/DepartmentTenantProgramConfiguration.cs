using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TaxBeacon.DAL.Configurations;

public class DepartmentTenantProgramConfiguration: IEntityTypeConfiguration<DepartmentTenantProgram>
{
    public void Configure(EntityTypeBuilder<DepartmentTenantProgram> departmentTenantProgram)
    {
        departmentTenantProgram
            .HasOne<Department>(dtp => dtp.Department)
            .WithMany(d => d.DepartmentTenantPrograms)
            .HasForeignKey(dtp => dtp.DepartmentId);

        departmentTenantProgram
            .HasOne<TenantProgram>(dtp => dtp.TenantProgram)
            .WithMany(tp => tp.DepartmentTenantPrograms)
            .HasForeignKey(dtp => new { dtp.TenantId, dtp.ProgramId });

        departmentTenantProgram
            .HasKey(dtp => new { dtp.TenantId, dtp.DepartmentId, dtp.ProgramId });

        departmentTenantProgram
            .Property(dtp => dtp.IsDeleted)
            .HasDefaultValue(false);
    }
}
