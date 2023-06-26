using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaxBeacon.DAL.Entities.Accounts;

namespace TaxBeacon.DAL.Configurations;

public class ClientManagerConfiguration: IEntityTypeConfiguration<ClientManager>
{
    public void Configure(EntityTypeBuilder<ClientManager> clientManager)
    {
        clientManager
            .HasOne<Client>(cm => cm.Client)
            .WithMany(c => c.ClientManagers)
            .HasForeignKey(cm => new { cm.TenantId, cm.AccountId });

        clientManager
            .HasOne<User>(cm => cm.Manager)
            .WithMany(u => u.ClientManagers)
            .HasForeignKey(cm => cm.ManagerId);

        clientManager
            .HasKey(cm => new { cm.ManagerId, cm.TenantId, cm.AccountId });
    }
}
