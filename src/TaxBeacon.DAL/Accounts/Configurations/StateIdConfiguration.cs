using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaxBeacon.DAL.Accounts.Entities;

namespace TaxBeacon.DAL.Accounts.Configurations;
public class StateIdConfiguration: IEntityTypeConfiguration<StateId>
{
    public void Configure(EntityTypeBuilder<StateId> builder)
    {
        builder
            .Property(e => e.Value)
            .HasColumnType("nvarchar")
            .HasMaxLength(100)
            .IsRequired();

        builder
            .Property(l => l.Id)
            .HasDefaultValueSql("NEWID()");

    }
}
