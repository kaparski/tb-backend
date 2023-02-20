namespace TaxBeacon.DAL.Entities;

public class RoleTenantUser
{
    public Guid RoleId { get; set; }

    public Guid TenantId { get; set; }

    public Guid UserId { get; set; }

    public Role Role { get; set; } = null!;

    public TenantUser TenantUser { get; set; } = null!;
}
