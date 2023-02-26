namespace TaxBeacon.DAL.Entities;

public class TenantRolePermission
{
    public Guid TenantId { get; set; }

    public int RoleId { get; set; }

    public int PermissionId { get; set; }

    public TenantPermission TenantPermission { get; set; }

    public TenantRole TenantRole { get; set; }
}
