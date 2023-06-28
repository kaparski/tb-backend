using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaxBeacon.DAL.Administration.Entities;

namespace TaxBeacon.DAL.Administration.Configurations;

public class UserActivityLogConfiguration: IEntityTypeConfiguration<UserActivityLog>
{
    public void Configure(EntityTypeBuilder<UserActivityLog> builder)
    {
        builder
            .HasOne<User>(ual => ual.User)
            .WithMany(u => u.UserActivityLogs)
            .HasForeignKey(ual => ual.UserId);

        builder
            .HasKey(ual => new { ual.TenantId, ual.UserId, ual.Date });

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
            .Property(ual => ual.UserId)
            .IsRequired();

        builder
            .Property(ual => ual.Date)
            .IsRequired();

        builder.HasQueryFilter(ua => ua.User.IsDeleted == null || !ua.User.IsDeleted.Value);
    }
}