using TaxBeacon.Common.Enums;

namespace TaxBeacon.DAL.Entities;

public class User: BaseEntity
{
    public Guid Id { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public UserStatus UserStatus { get; set; }

    //  TODO: Rename such columns to LastLoginDateTimeUtc
    public DateTime? LastLoginDateUtc { get; set; }

    public string FullName { get; private set; } = string.Empty;

    public DateTime? DeactivationDateTimeUtc { get; set; }

    public DateTime? ReactivationDateTimeUtc { get; set; }

    // TODO: Add department, roles and Job title in the future
    public ICollection<TenantUser> TenantUsers { get; set; } = new HashSet<TenantUser>();

    public ICollection<UserActivityLog> UserActivityLogs { get; set; } = new HashSet<UserActivityLog>();
}
