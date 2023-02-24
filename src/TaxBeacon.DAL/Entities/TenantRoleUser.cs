namespace TaxBeacon.DAL.Entities;

public class TenantRoleUser
{
    public Guid RoleId { get; set; }

    public Guid TenantId { get; set; }

    public Guid UserId { get; set; }

    public TenantUser TenantUser { get; set; } = null!;

    public TenantRole TenantRole { get; set; } = null!;
}
