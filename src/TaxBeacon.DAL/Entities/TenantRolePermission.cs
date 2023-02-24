namespace TaxBeacon.DAL.Entities;

public class TenantRolePermission
{
    public Guid RoleId { get; set; }

    public Guid TenantId { get; set; }

    public Guid PermissionId { get; set; }

    public TenantRole TenantRole { get; set; } = null!;

    public TenantPermission TenantPermission { get; set; } = null!;
}
