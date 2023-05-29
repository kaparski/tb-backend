using Mapster;
using TaxBeacon.Common.Enums;
using TaxBeacon.DAL.Entities;

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

    public string FullName { get; init; } = null!;

    public DateTime? DeactivationDateTimeUtc { get; set; }

    public DateTime? ReactivationDateTimeUtc { get; set; }

    public string Roles { get; set; } = null!;
}
