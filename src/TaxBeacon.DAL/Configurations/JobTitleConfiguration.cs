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
                .HasMaxLength(100)
                .IsRequired();

            jobTitle
                .Property(sa => sa.Description)
                .HasColumnType("nvarchar")
                .HasMaxLength(200)
                .IsRequired(false);

            jobTitle
                .HasKey(jt => jt.Id)
                .IsClustered(false);

            jobTitle
                .HasIndex(jt => new { jt.TenantId, jt.DepartmentId, jt.Id })
                .IsClustered();

            jobTitle
                .HasIndex(jt => new { jt.TenantId, jt.Name })
                .IsUnique();

            jobTitle
                .HasMany(jt => jt.Users)
                .WithOne(u => u.JobTitle)
                .HasForeignKey(u => u.JobTitleId);

            jobTitle
                .HasOne(d => d.Tenant)
                .WithMany(t => t.JobTitles)
                .HasForeignKey(d => d.TenantId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
