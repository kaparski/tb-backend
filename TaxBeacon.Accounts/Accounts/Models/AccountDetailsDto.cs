using TaxBeacon.Common.Enums;

namespace TaxBeacon.Accounts.Accounts.Models;

public record AccountDetailsDto
{
    public Guid Id { get; init; }

    public string Name { get; init; } = null!;

    public string? DoingBusinessAs { get; init; }

    public string? LinkedInUrl { get; init; }

    public string Website { get; init; } = null!;

    public string Country { get; init; } = null!;

    public string? StreetAddress1 { get; init; }

    public string? StreetAddress2 { get; init; }

    public string? City { get; init; }

    public State? State { get; init; }

    public int? Zip { get; init; }

    public string? County { get; init; }

    public int? Phone { get; init; }

    public string? Extension { get; init; }

    public int? Fax { get; init; }

    public string? Address { get; init; }

    public IEnumerable<SalesPersonDto> SalesPersons { get; init; } = Enumerable.Empty<SalesPersonDto>();
}
