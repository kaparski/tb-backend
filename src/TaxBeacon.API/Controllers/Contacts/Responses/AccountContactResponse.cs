using TaxBeacon.API.Shared.Responses;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.API.Controllers.Contacts.Responses;

public record AccountContactResponse
{
    public Guid Id { get; init; }

    public string FullName { get; set; } = null!;

    public string FirstName { get; init; } = null!;

    public string LastName { get; init; } = null!;

    public string Email { get; init; } = null!;

    public string? SecondaryEmail { get; init; } = null!;

    public string? JobTitle { get; init; }

    public DateTime CreatedDateTimeUtc { get; init; }

    public DateTime? LastModifiedDateTimeUtc { get; init; }

    public Status Status { get; init; }

    public string Type { get; init; } = null!;

    public DateTime? ReactivationDateTimeUtc { get; init; }

    public DateTime? DeactivationDateTimeUtc { get; init; }

    public IEnumerable<PhoneResponse> Phones { get; init; } = Enumerable.Empty<PhoneResponse>();

    public IEnumerable<ContactAccountResponse> Accounts { get; init; } = Enumerable.Empty<ContactAccountResponse>();

    public IEnumerable<LinkedContactResponse> LinkedContacts { get; init; } = Enumerable.Empty<LinkedContactResponse>();

}
