namespace TaxBeacon.DAL.Entities;

public class Permission
{
    public Guid Id { get; set; }

    public PermissionEnum Name { get; set; }

    public ICollection<TenantPermission> TenantPermissions { get; set; } = new HashSet<TenantPermission>();
}

public enum PermissionEnum
{
    Login = 1,
    CreateUser = 2,
    ReadListOfUsers = 3,
    ReadUserDetails = 4,
    UpdateUserStatus = 5
}
