using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaxBeacon.DAL.Accounts.Entities;

namespace TaxBeacon.DAL.Accounts.Configurations;

public class PhoneConfiguration: IEntityTypeConfiguration<Phone>
{
    public void Configure(EntityTypeBuilder<Phone> builder)
    {
        builder
            .Property(t => t.Id)
            .HasDefaultValueSql("NEWID()");

        builder
            .HasOne<Account>(p => p.Account)
            .WithMany(a => a.Phones)
            .HasForeignKey(p => p.AccountId);

        builder
            .Property(a => a.Number)
            .HasColumnType("nvarchar")
            .HasMaxLength(15)
            .IsRequired();

        builder
            .Property(a => a.Type)
            .HasColumnType("nvarchar")
            .HasMaxLength(10)
            .IsRequired();

        builder
            .Property(a => a.Extension)
            .HasColumnType("nvarchar")
            .HasMaxLength(200);
    }
}
