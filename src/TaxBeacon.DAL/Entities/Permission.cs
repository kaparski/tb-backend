namespace TaxBeacon.DAL.Entities;

public class Permission: BaseEntity
{
    public Guid Id { get; init; }

    public string Name { get; init; } = null!;

    public ICollection<TenantPermission> TenantPermissions { get; set; } = new HashSet<TenantPermission>();

    public ICollection<RolePermission> RolePermissions { get; set; } = new HashSet<RolePermission>();
}
