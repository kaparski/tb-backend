namespace TaxBeacon.API.Controllers.Contacts.Responses;

public record LinkedContactDetailsResponse
{
    public Guid Id { get; init; }

    public Guid SourceContactId { get; init; }

    public string Email { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string FirstName { get; init; } = string.Empty;

    public string LastName { get; init; } = string.Empty;

    public string Comment { get; init; } = string.Empty;

    public IEnumerable<string> AccountNames { get; init; } = Enumerable.Empty<string>();
}
