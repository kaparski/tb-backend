using TaxBeacon.Common.Enums;

namespace TaxBeacon.DAL.Administration.Entities;

/// <summary>
/// Represents a record in TenantUsersView view. Must be separate from User entity, otherwise EF gets confused and produces wrong queries.
/// </summary>
public class TenantUserView: BaseDeletableEntity
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string FirstName { get; set; } = null!;

    public string LegalName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public Status Status { get; set; }

    public DateTime? LastLoginDateTimeUtc { get; set; }

    public string FullName { get; private set; } = null!;

    public DateTime? DeactivationDateTimeUtc { get; set; }

    public DateTime? ReactivationDateTimeUtc { get; set; }

    public Guid? DivisionId { get; set; }

    public string? Division { get; set; }

    public Guid? DepartmentId { get; set; }

    public string? Department { get; set; }

    public Guid? ServiceAreaId { get; set; }

    public string? ServiceArea { get; set; }

    public Guid? JobTitleId { get; set; }

    public string? JobTitle { get; set; }

    public Guid? TeamId { get; set; }

    public string? Team { get; set; }

    public string? Roles { get; set; }

    public string? UserIdPlusTenantId { get; set; }
}
