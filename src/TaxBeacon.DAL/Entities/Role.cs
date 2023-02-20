namespace TaxBeacon.DAL.Entities;

public class Role
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public ICollection<RoleTenantPermission> RoleTenantPermissions { get; set; } = new HashSet<RoleTenantPermission>();

    public ICollection<RoleTenantUser> RoleTenantUsers { get; set; } = new HashSet<RoleTenantUser>();
}
