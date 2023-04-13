using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TaxBeacon.DAL.Configurations
{
    public class DivisionConfiguration: IEntityTypeConfiguration<Division>
    {
        public void Configure(EntityTypeBuilder<Division> division)
        {
            division
                .Property(d => d.Name)
                .HasColumnType("nvarchar")
                .HasMaxLength(50)
                .IsRequired();

            division
                .Property(d => d.Description)
                .HasColumnType("nvarchar")
                .HasMaxLength(200)
                .IsRequired(false);

            division
                .HasKey(d => d.Id)
                .IsClustered(false);

            division
                .HasIndex(d => new { d.TenantId, d.Id })
                .IsClustered();

            division
                .HasOne(d => d.Tenant)
                .WithMany(t => t.Divisions)
                .HasForeignKey(d => d.TenantId);

            division
                .HasIndex(d => new { d.TenantId, d.Name })
                .IsUnique();

            division
                .HasMany(d => d.Users)
                .WithOne(u => u.Division)
                .HasForeignKey(u => u.DivisionId);

            division
                .HasMany(d => d.Departments)
                .WithOne(dep => dep.Division)
                .HasForeignKey(dep => dep.DivisionId);
        }
    }
}
