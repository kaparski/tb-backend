using System.Collections;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.DAL.Entities;

public class Role: BaseEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public SourceType Type { get; set; }

    public ICollection<TenantRole> TenantRoles { get; set; } = new HashSet<TenantRole>();

    public ICollection<UserRole> UserRoles { get; set; } = new HashSet<UserRole>();

    public ICollection<RolePermission> RolePermissions { get; set; } = new HashSet<RolePermission>();
}
