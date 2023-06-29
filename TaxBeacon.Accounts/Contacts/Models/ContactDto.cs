using TaxBeacon.Common.Enums;

namespace TaxBeacon.Accounts.Contacts.Models;

public class ContactDto
{
    public Guid Id { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? JobTitle { get; set; }

    public string ContactType { get; set; } = null!;

    public string? Phone { get; set; }

    public Status Status { get; set; }

    public string? Country { get; set; }

    public string? City { get; set; }

    public State? State { get; set; }

    public Guid AccountId { get; set; }
    
    public string AccountName { get; set; } = null!;
}
