using TaxBeacon.Common.Enums;

namespace TaxBeacon.UserManagement.Services.Contacts.Models;

public class ContactDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string JobTitle { get; set; } = null!;

    public string ContactType { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public Status Status { get; set; }

    public string Country { get; set; } = null!;

    public string City { get; set; } = null!;

    public string State { get; set; } = null!;
}
