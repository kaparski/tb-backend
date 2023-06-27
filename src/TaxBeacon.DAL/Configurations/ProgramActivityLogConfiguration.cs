using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TaxBeacon.DAL.Configurations;

public class ProgramActivityLogConfiguration: IEntityTypeConfiguration<ProgramActivityLog>
{
    public void Configure(EntityTypeBuilder<ProgramActivityLog> builder)
    {
        builder
            .HasOne<Program>(x => x.Program)
            .WithMany(x => x.ProgramActivityLogs)
            .HasForeignKey(x => x.ProgramId);

        builder
            .HasOne<Tenant>(x => x.Tenant)
            .WithMany(x => x.ProgramActivityLogs)
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.ClientNoAction)
            .IsRequired(false);

        builder
            .HasKey(x => new { x.ProgramId, x.Date });

        builder
            .Property(x => x.Event)
            .HasColumnType("nvarchar(max)")
            .IsRequired();

        builder
            .Property(x => x.EventType)
            .IsRequired();

        builder
            .Property(x => x.Revision)
            .IsRequired();

        builder
            .Property(x => x.ProgramId)
            .IsRequired();

        builder
            .Property(x => x.Date)
            .IsRequired();
    }
}