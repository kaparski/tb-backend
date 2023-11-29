namespace TaxBeacon.DAL.Administration.Entities;

public class TenantUser
{
    public Guid UserId { get; set; }

    public Guid TenantId { get; set; }

    public User User { get; set; } = null!;

    public Tenant Tenant { get; set; } = null!;

    public ICollection<TenantUserRole> TenantUserRoles { get; set; } = new HashSet<TenantUserRole>();

    public ICollection<AccountSalesperson> TenantUserAccounts { get; set; } = new HashSet<AccountSalesperson>();

    public ICollection<ClientManager> ClientManagers { get; set; } = new HashSet<ClientManager>();

    public ICollection<ReferralManager> ReferralManagers { get; set; } = new HashSet<ReferralManager>();

    public ICollection<Document> Documents { get; set; } = new HashSet<Document>();
}
