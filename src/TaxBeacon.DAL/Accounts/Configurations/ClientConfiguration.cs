using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaxBeacon.DAL.Accounts.Entities;

namespace TaxBeacon.DAL.Accounts.Configurations;
public class ClientConfiguration: IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> client)
    {
        client
            .HasKey(c => new { c.TenantId, c.AccountId });

        client
            .HasOne(c => c.Tenant)
            .WithMany(t => t.Clients)
            .HasForeignKey(c => c.TenantId)
            .OnDelete(DeleteBehavior.NoAction);

        client
            .HasOne(c => c.Account)
            .WithOne(a => a.Client)
            .HasForeignKey<Client>(c => c.AccountId);

        client
            .HasOne(c => c.PrimaryContact)
            .WithMany(m => m.Clients)
            .HasForeignKey(c => c.PrimaryContactId);

        client
            .Property(c => c.Status)
            .HasConversion<string>()
            .HasColumnType("nvarchar")
            .HasMaxLength(100);

        client
            .Property(c => c.State)
            .HasColumnType("nvarchar")
            .HasMaxLength(50)
            .IsRequired();

        client
            .Property(c => c.AnnualRevenue)
            .HasColumnType("decimal")
            .HasPrecision(15, 2);
    }
}
