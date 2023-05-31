using TaxBeacon.Common.Enums;

namespace TaxBeacon.UserManagement.Models;

public class UserDto
{
    public Guid Id { get; set; }

    public string FirstName { get; set; } = null!;

    public string LegalName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public DateTime CreatedDateTimeUtc { get; set; }

    public string Email { get; set; } = null!;

    public Status Status { get; set; }

    public DateTime? LastLoginDateTimeUtc { get; set; }

    public string FullName { get; init; } = string.Empty;

    public DateTime? DeactivationDateTimeUtc { get; set; }

    public DateTime? ReactivationDateTimeUtc { get; set; }

    public string? Roles { get; set; }

    public string? RoleNamesAsString { get; set; }

    public string? RoleIdsAsString { get; set; }

    public IEnumerable<Guid>? RoleIds { get; set; }

    public IEnumerable<string>? RoleNames { get; set; }

    public string? Department { get; set; }

    public string? Division { get; set; }

    public string? JobTitle { get; set; }

    public string? TeamName { get; set; }
}
