using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TaxBeacon.DAL.Configurations
{
    public class JobTitleConfiguration: IEntityTypeConfiguration<JobTitle>
    {
        public void Configure(EntityTypeBuilder<JobTitle> jobTitle)
        {
            jobTitle
                .Property(jt => jt.Name)
                .HasColumnType("nvarchar")
                .HasMaxLength(50)
                .IsRequired();

            jobTitle
                .HasKey(jt => jt.Id)
                .IsClustered(false);

            jobTitle
                .HasIndex(jt => new { jt.TenantId, jt.DepartmentId, jt.Id })
                .IsClustered();

            jobTitle
                .HasIndex(jt => new { jt.TenantId, jt.Name })
                .IsUnique();
        }
    }
}
