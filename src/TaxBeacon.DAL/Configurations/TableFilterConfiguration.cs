using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaxBeacon.DAL.Entities;

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
            .HasKey(tf => tf.Id)
            .IsClustered(false);

        tableFilter
            .HasIndex(tf => new { tf.TenantId, tf.TableType, tf.UserId })
            .IsClustered();

        tableFilter
            .HasOne<Tenant>(tf => tf.Tenant)
            .WithMany(t => t.TableFilters)
            .HasForeignKey(tf => tf.TenantId);

        tableFilter
            .HasOne<User>(tf => tf.User)
            .WithMany(u => u.TableFilters)
            .HasForeignKey(tf => tf.UserId);

        tableFilter
            .Property(p => p.Id)
            .HasDefaultValueSql("NEWID()");

        tableFilter.HasQueryFilter(tf => tf.User.IsDeleted == null || !tf.User.IsDeleted.Value);

    }
}
