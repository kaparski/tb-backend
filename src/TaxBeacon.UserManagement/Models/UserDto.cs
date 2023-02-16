using TaxBeacon.Common.Enums;

namespace TaxBeacon.UserManagement.Models;

public class UserDto
{
    public Guid Id { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public DateTime CreatedDateUtc { get; set; }

    public string Email { get; set; } = null!;

    public UserStatus UserStatus { get; set; }

    public DateTime? LastLoginDateUtc { get; set; }

    public string FullName { get; set; } = null!;

    public DateTime? DeactivationDateTimeUtc { get; set; }

    public DateTime? ReactivationDateTimeUtc { get; set; }
}
