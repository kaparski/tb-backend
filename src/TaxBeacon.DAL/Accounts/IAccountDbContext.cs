using Microsoft.EntityFrameworkCore;
using TaxBeacon.DAL.Accounts.Entities;

namespace TaxBeacon.DAL.Accounts;

public interface IAccountDbContext
{
    DbSet<Account> Accounts { get; }

    DbSet<Client> Clients { get; }

    DbSet<Referral> Referrals { get; }

    DbSet<Entity> Entities { get; }

    DbSet<Location> Locations { get; }

    DbSet<Contact> Contacts { get; }

    DbSet<AccountView> AccountsView { get; }

    DbSet<StateId> StateIds { get; }

    DbSet<EntityActivityLog> EntityActivityLogs { get; }

    DbSet<ClientManager> ClientManagers { get; }

    DbSet<ContactActivityLog> ContactActivityLogs { get; }

    DbSet<AccountActivityLog> AccountActivityLogs { get; }

    DbSet<AccountSalesperson> Salespersons { get; }

    DbSet<Phone> Phones { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
