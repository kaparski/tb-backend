﻿namespace TaxBeacon.DAL.Administration.Entities;

public class TenantRole
{
    public Guid TenantId { get; set; }

    public Guid RoleId { get; set; }

    public Tenant Tenant { get; set; } = null!;

    public Role Role { get; set; } = null!;

    public ICollection<TenantRolePermission> TenantRolePermissions { get; set; } = new HashSet<TenantRolePermission>();

    public ICollection<TenantUserRole> TenantUserRoles { get; set; } = new HashSet<TenantUserRole>();
}
