namespace TaxBeacon.DAL.Accounts.Entities;
public class ReferralManager
{
    public Guid UserId { get; set; }

    public Guid AccountId { get; set; }

    public Guid TenantId { get; set; }

    public TenantUser TenantUser { get; set; } = null!;

    public Referral Referral { get; set; } = null!;
}
