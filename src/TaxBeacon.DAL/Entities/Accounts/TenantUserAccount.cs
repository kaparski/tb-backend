namespace TaxBeacon.DAL.Entities.Accounts;

public class TenantUserAccount
{
    public Guid TenantId { get; set; }

    public Guid UserId { get; set; }

    public Guid AccountId { get; set; }

    public TenantUser TenantUser { get; set; } = null!;

    public Account Account { get; set; } = null!;
}
