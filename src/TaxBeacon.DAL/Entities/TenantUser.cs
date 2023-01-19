namespace TaxBeacon.DAL.Entities;

public class TenantUser
{
    public Guid UserId { get; set; }

    public Guid TenantId { get; set; }

    public User User { get; set; }

    public Tenant Tenant { get; set; }
}
