using TaxBeacon.DAL.Entities.Accounts;

namespace TaxBeacon.DAL.Entities;

public class TenantUser
{
    public Guid UserId { get; set; }

    public Guid TenantId { get; set; }

    public User User { get; set; } = null!;

    public Tenant Tenant { get; set; } = null!;

    public ICollection<TenantUserRole> TenantUserRoles { get; set; } = new HashSet<TenantUserRole>();

    public ICollection<TenantUserAccount> TenantUserAccounts { get; set; } = new HashSet<TenantUserAccount>();

    public ICollection<ClientManager> ClientManagers { get; set; } = new HashSet<ClientManager>();

    public ICollection<Referral> Referrals { get; set; } = new HashSet<Referral>();
}
