using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Permissions;

namespace TaxBeacon.UserManagement.Models;

public class UserDetailsDto
{
    public Guid Id { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public DateTime CreatedDateUtc { get; set; }

    public string Email { get; set; } = null!;

    public Status UserStatus { get; set; }

    public DateTime? LastLoginDateUtc { get; set; }

    public string FullName { get; private set; } = string.Empty;

    public DateTime? DeactivationDateTimeUtc { get; set; }

    public DateTime? ReactivationDateTimeUtc { get; set; }

    public List<RoleDto> Roles { get; set; } = new();
}
