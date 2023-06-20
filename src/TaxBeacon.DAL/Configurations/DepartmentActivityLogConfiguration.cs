using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TaxBeacon.DAL.Configurations;

public class DepartmentActivityLogConfiguration: IEntityTypeConfiguration<DepartmentActivityLog>
{
    public void Configure(EntityTypeBuilder<DepartmentActivityLog> builder)
    {
        builder
            .HasOne<Department>(x => x.Department)
            .WithMany(x => x.DepartmentActivityLogs)
            .HasForeignKey(x => x.DepartmentId);

        builder
            .HasOne<Tenant>(x => x.Tenant)
            .WithMany(x => x.DepartmentActivityLogs)
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.ClientNoAction);

        builder
            .HasKey(ual => new { ual.TenantId, ual.DepartmentId, ual.Date });

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
            .Property(ual => ual.DepartmentId)
            .IsRequired();

        builder
            .Property(ual => ual.Date)
            .IsRequired();
    }
}
