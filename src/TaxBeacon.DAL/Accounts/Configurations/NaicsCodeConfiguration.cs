using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaxBeacon.DAL.Accounts.Entities;

namespace TaxBeacon.DAL.Accounts.Configurations;
public class NaicsCodeConfiguration: IEntityTypeConfiguration<NaicsCode>
{
    public void Configure(EntityTypeBuilder<NaicsCode> builder)
    {
        builder
            .HasKey(b => b.Code);

        builder
            .Property(b => b.Code)
            .ValueGeneratedNever();

        builder
            .Property(b => b.Title)
            .HasColumnType("nvarchar")
            .HasMaxLength(200);

        builder
            .Property(b => b.Description)
            .HasColumnType("nvarchar(max)");

        builder
            .HasOne(b => b.Parent)
            .WithMany(b => b.Children)
            .HasForeignKey(b => b.ParentCode)
            .IsRequired(false);
    }
}
