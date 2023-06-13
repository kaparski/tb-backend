using TaxBeacon.Common.Accounts;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.API.Controllers.Contacts.Responses;

public class ContactResponse
{
    public Guid Id { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string JobTitle { get; set; } = null!;

    public string ContactType { get; set; } = string.Empty;

    public string Phone { get; set; } = null!;

    public Status Status { get; set; }

    public string Country { get; set; } = null!;

    public string City { get; set; } = null!;

    public State State { get; set; } = State.None;
}
