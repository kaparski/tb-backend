using TaxBeacon.Common.Enums;

namespace TaxBeacon.DAL.Entities;

/// <summary>
/// Represents a record in UsersView view. Must be separate from User entity, otherwise EF gets confused and produces wrong queries.
/// </summary>
public class UserView: BaseEntity
{
    public Guid Id { get; set; }

    public Guid? TenantId { get; set; }

    public string FirstName { get; set; } = null!;

    public string LegalName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public Status Status { get; set; }

    public DateTime? LastLoginDateTimeUtc { get; set; }

    public string FullName { get; private set; } = null!;

    public DateTime? DeactivationDateTimeUtc { get; set; }

    public DateTime? ReactivationDateTimeUtc { get; set; }

    public string? Division { get; set; }

    public string? Department { get; set; }

    public string? ServiceArea { get; set; }

    public string? JobTitle { get; set; }

    public string? TeamName { get; set; }

    public string? Roles { get; set; }

    public string? RoleIdsAsString { get; set; }

    public string? RoleNamesAsString { get; set; }

    public string? UserIdPlusTenantId { get; set; }
}
