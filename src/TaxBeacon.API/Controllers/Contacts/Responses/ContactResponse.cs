using TaxBeacon.API.Shared.Responses;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.API.Controllers.Contacts.Responses;

public record ContactResponse
{
    public Guid Id { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? SecondaryEmail { get; init; } = null!;

    public string? JobTitle { get; set; }

    public DateTime CreatedDateTimeUtc { get; init; }

    public DateTime? LastModifiedDateTimeUtc { get; init; }

    public IEnumerable<ContactAccountResponse> Accounts { get; init; } = Enumerable.Empty<ContactAccountResponse>();

    public IEnumerable<LinkedContactResponse> LinkedContacts { get; init; } = Enumerable.Empty<LinkedContactResponse>();

    public IEnumerable<PhoneResponse> Phones { get; init; } = Enumerable.Empty<PhoneResponse>();
}
