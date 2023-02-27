using TaxBeacon.Common.Enums;

namespace TaxBeacon.DAL.Entities;

public class Permission: BaseEntity
{
    public Guid Id { get; init; }

    public PermissionEnum Name { get; init; }

    public ICollection<TenantPermission> TenantPermissions { get; set; } = new HashSet<TenantPermission>();
}
