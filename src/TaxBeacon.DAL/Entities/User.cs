using TaxBeacon.Common.Enums;

namespace TaxBeacon.DAL.Entities;

public class User: BaseEntity
{
    public Guid Id { get; set; }

    public string Username { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public UserStatus UserStatus { get; set; }

    public DateTime? LastLoginDateUtc { get; set; }

    // TODO: Add department, roles and Job title in the future
    public ICollection<TenantUser> TenantUsers { get; set; } = new HashSet<TenantUser>();
}
