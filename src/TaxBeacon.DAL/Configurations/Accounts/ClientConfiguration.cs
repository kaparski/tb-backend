using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NPOI.SS.Formula.Eval;
using TaxBeacon.DAL.Entities.Accounts;

namespace TaxBeacon.DAL.Configurations.Accounts;
public class ClientConfiguration: IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> client)
    {
        client
            .HasKey(c => new { c.TenantId, c.AccountId });

        client.
            HasOne(c => c.Tenant)
            .WithMany(t => t.Clients)
            .HasForeignKey(c => c.TenantId);

        client
            .HasOne(c => c.Account)
            .WithOne(a => a.Client)
            .HasForeignKey<Client>(c => c.AccountId);

        client
            .HasOne(c => c.Manager)
            .WithMany(m => m.Clients)
            .HasForeignKey(c => c.ManagerId);
    }
}
