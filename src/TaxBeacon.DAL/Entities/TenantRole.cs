namespace TaxBeacon.DAL.Entities;

public class TenantRole
{
    public Guid RoleId { get; set; }

    public Guid TenantId { get; set; }

    public Role Role { get; set; } = null!;

    public Tenant Tenant { get; set; } = null!;

    public ICollection<TenantRolePermission> TenantRolePermissions { get; set; } = null!;

    public ICollection<TenantRoleUser> TenantRoleUsers { get; set; } = null!;
}
