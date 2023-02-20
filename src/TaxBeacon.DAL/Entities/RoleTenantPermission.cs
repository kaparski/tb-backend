namespace TaxBeacon.DAL.Entities;

public class RoleTenantPermission
{
    public Guid RoleId { get; set; }

    public Guid TenantId { get; set; }

    public Guid PermissionId { get; set; }

    public Role Role { get; set; } = null!;

    public TenantPermission TenantPermission { get; set; } = null!;
}
