namespace TaxBeacon.DAL.Administration.Entities;

public class TenantRolePermission
{
    public Guid TenantId { get; set; }

    public Guid RoleId { get; set; }

    public Guid PermissionId { get; set; }

    public TenantPermission TenantPermission { get; set; } = null!;

    public TenantRole TenantRole { get; set; } = null!;
}
