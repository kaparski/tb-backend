namespace TaxBeacon.DAL.Entities;

public class TenantPermission
{
    public Guid TenantId { get; set; }

    public Guid PermissionId { get; set; }

    public Permission Permission { get; set; } = null!;

    public Tenant Tenant { get; set; } = null!;

    public ICollection<TenantRolePermission> TenantRolePermissions { get; set; } = new HashSet<TenantRolePermission>();
}
