namespace TaxBeacon.Accounts.Contacts.Models;

public record LinkedContactDetailsDto
{
    public Guid Id { get; init; }

    public Guid SourceContactId { get; init; }

    public string Email { get; init; } = string.Empty;

    public string FirstName { get; init; } = string.Empty;

    public string LastName { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string Comment { get; init; } = string.Empty;

    public IEnumerable<string> AccountNames { get; set; } = Enumerable.Empty<string>();
}
