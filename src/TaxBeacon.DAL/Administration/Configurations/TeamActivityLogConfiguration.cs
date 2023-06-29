using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaxBeacon.DAL.Administration.Entities;

namespace TaxBeacon.DAL.Administration.Configurations;

public class TeamActivityLogConfiguration: IEntityTypeConfiguration<TeamActivityLog>
{
    public void Configure(EntityTypeBuilder<TeamActivityLog> builder)
    {
        builder
            .HasOne<Team>(x => x.Team)
            .WithMany(x => x.TeamActivityLogs)
            .HasForeignKey(x => x.TeamId);

        builder
            .HasOne<Tenant>(x => x.Tenant)
            .WithMany(x => x.TeamActivityLogs)
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.ClientNoAction);

        builder
            .HasKey(ual => new { ual.TenantId, ual.TeamId, ual.Date });

        builder
            .Property(ual => ual.Event)
            .HasColumnType("nvarchar(max)")
            .IsRequired();

        builder
            .Property(ual => ual.EventType)
            .IsRequired();

        builder
            .Property(ual => ual.Revision)
            .IsRequired();

        builder
            .Property(ual => ual.TenantId)
            .IsRequired();

        builder
            .Property(ual => ual.TeamId)
            .IsRequired();

        builder
            .Property(ual => ual.Date)
            .IsRequired();
    }
}