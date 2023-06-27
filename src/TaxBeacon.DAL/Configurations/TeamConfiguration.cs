using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TaxBeacon.DAL.Configurations;

public class TeamConfiguration: IEntityTypeConfiguration<Team>
{
    public void Configure(EntityTypeBuilder<Team> team)
    {
        team
            .Property(t => t.Name)
            .HasColumnType("nvarchar")
            .HasMaxLength(100)
            .IsRequired();

        team
            .HasKey(t => t.Id)
            .IsClustered(false);

        team
            .HasIndex(t => new { t.TenantId, t.Id })
            .IsClustered();

        team
            .HasIndex(t => new { t.TenantId, t.Name })
            .IsUnique();

        team
            .HasOne(t => t.Tenant)
            .WithMany(tenant => tenant.Teams)
            .HasForeignKey(t => t.TenantId);

        team
            .HasMany(t => t.Users)
            .WithOne(u => u.Team)
            .HasForeignKey(u => u.TeamId);

        team
            .Property(t => t.Description)
            .HasColumnType("nvarchar")
            .HasMaxLength(200)
            .IsRequired(false);
    }
}