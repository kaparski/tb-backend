namespace TaxBeacon.DAL.Administration.Entities;

public class Permission: BaseDeletableEntity
{
    public Guid Id { get; init; }

    public string Name { get; init; } = null!;

    public ICollection<TenantPermission> TenantPermissions { get; set; } = new HashSet<TenantPermission>();

    public ICollection<RolePermission> RolePermissions { get; set; } = new HashSet<RolePermission>();
}
