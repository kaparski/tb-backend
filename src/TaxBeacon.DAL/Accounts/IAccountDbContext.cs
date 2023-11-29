using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using TaxBeacon.DAL.Accounts.Entities;

namespace TaxBeacon.DAL.Accounts;

public interface IAccountDbContext
{
    DbSet<Account> Accounts { get; }

    DbSet<Client> Clients { get; }

    DbSet<Referral> Referrals { get; }

    DbSet<Entity> Entities { get; }

    DbSet<Location> Locations { get; }

    DbSet<Document> Documents { get; }

    DbSet<AccountDocument> AccountDocuments { get; }

    DbSet<Contact> Contacts { get; }

    DbSet<AccountView> AccountsView { get; }

    DbSet<ClientView> ClientsView { get; }

    DbSet<ReferralView> ReferralsView { get; }

    DbSet<StateId> StateIds { get; }

    DbSet<EntityActivityLog> EntityActivityLogs { get; }

    DbSet<ClientManager> ClientManagers { get; }

    DbSet<ReferralManager> ReferralManagers { get; }

    DbSet<ContactActivityLog> ContactActivityLogs { get; }

    DbSet<AccountActivityLog> AccountActivityLogs { get; }

    DbSet<AccountSalesperson> Salespersons { get; }

    DbSet<NaicsCode> NaicsCodes { get; }

    DbSet<LocationPhone> LocationPhones { get; }

    DbSet<EntityLocation> EntityLocations { get; }

    DbSet<LocationActivityLog> LocationActivityLogs { get; }

    DbSet<AccountPhone> AccountPhones { get; }

    DbSet<EntityPhone> EntityPhones { get; }

    DbSet<AccountContact> AccountContacts { get; }

    DbSet<ContactPhone> ContactPhones { get; }

    DbSet<LinkedContact> LinkedContacts { get; }

    DbSet<AccountContactActivityLog> AccountContactActivityLogs { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    ChangeTracker ChangeTracker { get; }
}
