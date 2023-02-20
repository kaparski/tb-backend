namespace TaxBeacon.DAL.Entities;

public class TenantUser
{
    public Guid UserId { get; set; }

    public Guid TenantId { get; set; }

    public User User { get; set; } = null!;

    public Tenant Tenant { get; set; } = null!;

    public ICollection<RoleTenantUser> RoleTenantUsers { get; set; } = new HashSet<RoleTenantUser>();
}
