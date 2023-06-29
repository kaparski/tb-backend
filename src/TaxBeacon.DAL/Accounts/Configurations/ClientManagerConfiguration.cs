using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaxBeacon.DAL.Accounts.Entities;
using TaxBeacon.DAL.Administration.Entities;

namespace TaxBeacon.DAL.Accounts.Configurations;

public class ClientManagerConfiguration: IEntityTypeConfiguration<ClientManager>
{
    public void Configure(EntityTypeBuilder<ClientManager> clientManager)
    {
        clientManager
            .HasOne<Client>(cm => cm.Client)
            .WithMany(c => c.ClientManagers)
            .HasForeignKey(cm => new { cm.TenantId, cm.AccountId })
            .OnDelete(DeleteBehavior.NoAction);

        clientManager
            .HasOne<TenantUser>(cm => cm.TenantUser)
            .WithMany(u => u.ClientManagers)
            .HasForeignKey(cm => new { cm.TenantId, cm.UserId })
            .OnDelete(DeleteBehavior.NoAction);

        clientManager
            .HasKey(cm => new { cm.TenantId, cm.AccountId, cm.UserId });
    }
}
