namespace TaxBeacon.DAL.Entities;

public class TenantUser
{
    public Guid UserId { get; set; }

    public Guid TenantId { get; set; }

    public User User { get; set; } = null!;

    public Tenant Tenant { get; set; } = null!;

    public ICollection<TenantUserRole> TenantUserRoles { get; set; } = new HashSet<TenantUserRole>();

    public ICollection<TableFilter> TableFilters { get; set; } = new HashSet<TableFilter>();
}
