using TaxBeacon.Common.Enums;

namespace TaxBeacon.DAL.Entities;

public class Permission
{
    public Guid Id { get; set; }

    public PermissionEnum Name { get; set; }

    public ICollection<TenantPermission> TenantPermissions { get; set; } = new HashSet<TenantPermission>();
}
