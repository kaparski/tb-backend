namespace TaxBeacon.DAL.Entities.Accounts;

public class Account: BaseEntity
{
    public Guid TenantId { get; set; }

    public Tenant Tenant { get; set; } = null!;

    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string? State { get; set; }

    public string? City { get; set; }

    public Client? Client { get; set; }

    public Referral? Referral { get; set; }
}
