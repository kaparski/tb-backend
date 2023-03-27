using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TaxBeacon.DAL.Configurations;

public class TableFilterConfiguration: IEntityTypeConfiguration<TableFilter>
{
    public void Configure(EntityTypeBuilder<TableFilter> tableFilter)
    {
        tableFilter
            .Property(tf => tf.Name)
            .HasColumnType("nvarchar")
            .HasMaxLength(50)
            .IsRequired();

        tableFilter
            .Property(tf => tf.Configuration)
            .HasColumnType("nvarchar(max)")
            .IsRequired();

        tableFilter
            .HasKey(tf => tf.Id);

        tableFilter
            .HasIndex(tf => new { tf.TenantId, tf.TableType, tf.UserId })
            .IsClustered();

        tableFilter
            .HasOne<TenantUser>(tf => tf.TenantUser)
            .WithMany(tu => tu.TableFilters)
            .HasForeignKey(tf => new { tf.TenantId, tf.UserId });

        tableFilter
            .Property(p => p.Id)
            .HasDefaultValueSql("NEWID()");
    }
}
