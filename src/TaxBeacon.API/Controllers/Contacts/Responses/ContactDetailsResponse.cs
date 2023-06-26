using TaxBeacon.Common.Enums;

namespace TaxBeacon.API.Controllers.Contacts.Responses;

public class ContactDetailsResponse
{
    public Guid Id { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? JobTitle { get; set; }

    public string Type { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string? Phone2 { get; set; } = null!;

    public Status Status { get; set; }

    public string? Country { get; set; }

    public string? City { get; set; }

    public string? Zip { get; set; }

    public string? Address { get; set; }

    public string? Role { get; set; }

    public string? SubRole { get; set; }

    public State State { get; set; } = State.None;

    public Guid AccountId { get; set; }

    public Guid TenantId { get; set; }

    public DateTime? DeactivationDateTimeUtc { get; set; }

    public DateTime? ReactivationDateTimeUtc { get; set; }
}
