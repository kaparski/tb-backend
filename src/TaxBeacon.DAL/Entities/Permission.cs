namespace TaxBeacon.DAL.Entities;

public class Permission: BaseEntity
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public ICollection<TenantPermission> TenantPermissions { get; set; } = new HashSet<TenantPermission>();
}
