namespace TaxBeacon.DAL.Entities;

public class TenantUserRole
{
    public Guid TenantId { get; set; }

    public int RoleId { get; set; }

    public Guid UserId { get; set; }

    public TenantRole TenantRole { get; set; } = null!;

    public TenantUser TenantUser { get; set; } = null!;
}
