using TaxBeacon.Common.Accounts;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.UserManagement.Services.Contacts.Models;

public class ContactDto
{
    public Guid Id { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? JobTitle { get; set; }

    public ContactType ContactType { get; set; } = ContactType.None;

    public string? Phone { get; set; }

    public Status Status { get; set; }

    public string? Country { get; set; }

    public string? City { get; set; } = null!;

    public State State { get; set; } = State.None;
}
