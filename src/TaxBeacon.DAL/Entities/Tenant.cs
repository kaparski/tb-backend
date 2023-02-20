namespace TaxBeacon.DAL.Entities;

public class Tenant: BaseEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public ICollection<TenantUser> TenantUsers { get; set; } = new HashSet<TenantUser>();

    public ICollection<TenantPermission> TenantPermissions { get; set; } = new HashSet<TenantPermission>();
}
