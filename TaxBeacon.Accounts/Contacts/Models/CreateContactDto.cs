using TaxBeacon.Accounts.Common.Models;
using TaxBeacon.Common.Enums.Accounts;

namespace TaxBeacon.Accounts.Contacts.Models;

public record CreateContactDto
{
    public string FirstName { get; init; } = null!;

    public string LastName { get; init; } = null!;

    public string Email { get; init; } = null!;

    public string? SecondaryEmail { get; init; }

    public string? JobTitle { get; init; }

    public ContactType Type { get; init; } = null!;

    public IEnumerable<CreateUpdatePhoneDto> Phones { get; init; } = Enumerable.Empty<CreateUpdatePhoneDto>();
}
