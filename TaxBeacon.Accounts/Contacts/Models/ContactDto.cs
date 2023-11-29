using TaxBeacon.Accounts.Common.Models;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.Accounts.Contacts.Models;

public record ContactDto
{
    public Guid Id { get; init; }

    public string FirstName { get; init; } = null!;

    public string LastName { get; init; } = null!;

    public string FullName { get; init; } = null!;

    public string Email { get; init; } = null!;

    public string? SecondaryEmail { get; init; } = null!;

    public string? JobTitle { get; init; }

    public DateTime CreatedDateTimeUtc { get; init; }

    public DateTime? LastModifiedDateTimeUtc { get; init; }

    public IEnumerable<ContactAccountDto> Accounts { get; init; } = Enumerable.Empty<ContactAccountDto>();

    public IEnumerable<LinkedContactDto> LinkedContacts { get; init; } = Enumerable.Empty<LinkedContactDto>();

    public IEnumerable<PhoneDto> Phones { get; init; } = Enumerable.Empty<PhoneDto>();
}
