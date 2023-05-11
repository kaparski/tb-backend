using TaxBeacon.Common.Enums;

namespace TaxBeacon.API.Controllers.Users.Responses;

public class UserResponse
{
    public Guid Id { get; set; }

    public string FirstName { get; set; } = null!;

    public string LegalName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public Status Status { get; set; }

    public DateTime CreatedDateTimeUtc { get; set; }

    public DateTime? LastLoginDateTimeUtc { get; set; }

    public string Email { get; set; } = null!;

    public string Roles { get; set; } = null!;

    public string Department { get; set; } = null!;

    public string JobTitle { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public DateTime? DeactivationDateTimeUtc { get; set; }

    public DateTime? ReactivationDateTimeUtc { get; set; }
}
