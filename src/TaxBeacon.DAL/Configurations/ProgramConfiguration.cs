using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TaxBeacon.DAL.Configurations
{
    public class ProgramConfiguration: IEntityTypeConfiguration<Program>
    {
        public void Configure(EntityTypeBuilder<Program> program)
        {
            program
                .Property(p => p.Name)
                .HasColumnType("nvarchar")
                .HasMaxLength(100)
                .IsRequired();

            program
                .HasIndex(p => p.Name)
                .IsUnique();

            program
                .Property(p => p.Id)
                .HasDefaultValueSql("NEWID()");

            program
                .HasKey(p => p.Id);

            program
                .Property(p => p.CreatedDateTimeUtc)
                .HasDefaultValueSql("GETUTCDATE()");

            program
                .Property(p => p.IncentivesType)
                .HasMaxLength(100);

            program
                .Property(p => p.IncentivesArea)
                .HasMaxLength(100);

            program
                .Property(p => p.Overview)
                .HasMaxLength(200);

            program
                .Property(p => p.Reference)
                .HasMaxLength(200);

            program
                .Property(p => p.LegalAuthority)
                .HasMaxLength(200);

            program
                .Property(p => p.Agency)
                .HasMaxLength(200)
                .IsRequired();

            program
                .Property(p => p.State)
                .HasMaxLength(200);

            program
                .Property(p => p.County)
                .HasMaxLength(200);

            program
                .Property(p => p.City)
                .HasMaxLength(200);

            program
                .Property(p => p.JurisdictionName)
                .HasColumnType("nvarchar")
                .HasMaxLength(604)
                .HasComputedColumnSql(
                    @"TRIM(CASE WHEN [Jurisdiction] = 1 THEN 'Federal' WHEN [Jurisdiction] = 2 THEN [State] WHEN [Jurisdiction] = 3 THEN CONCAT_WS(', ',[State], [County], [City]) ELSE NULL END)",
                    stored: true);
        }
    }
}
