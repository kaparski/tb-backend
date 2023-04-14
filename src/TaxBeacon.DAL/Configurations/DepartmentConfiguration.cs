using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TaxBeacon.DAL.Configurations
{
    public class DepartmentConfiguration: IEntityTypeConfiguration<Department>
    {
        public void Configure(EntityTypeBuilder<Department> department)
        {
            department
                .Property(d => d.Name)
                .HasColumnType("nvarchar")
                .HasMaxLength(50)
                .IsRequired();

            department
                .Property(d => d.Description)
                .HasColumnType("nvarchar")
                .HasMaxLength(200)
                .IsRequired(false);

            department
                .HasKey(d => d.Id)
                .IsClustered(false);

            department
                .HasIndex(d => new { d.TenantId, d.DivisionId, d.Id })
                .IsClustered();

            department
                .HasIndex(d => new { d.TenantId, d.Name })
                .IsUnique();

            department
                .HasOne(d => d.Tenant)
                .WithMany(t => t.Departments)
                .HasForeignKey(d => d.TenantId)
                .OnDelete(DeleteBehavior.NoAction);

            department
                .HasMany(d => d.Users)
                .WithOne(u => u.Department)
                .HasForeignKey(u => u.DepartmentId);

            department
                .HasMany(d => d.JobTitles)
                .WithOne(jt => jt.Department)
                .HasForeignKey(jt => jt.DepartmentId)
                .IsRequired(false);

            department
                .HasMany(d => d.ServiceAreas)
                .WithOne(s => s.Department)
                .HasForeignKey(s => s.DepartmentId)
                .IsRequired(false);
        }
    }
}
