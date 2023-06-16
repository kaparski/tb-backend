using TaxBeacon.Common.Enums;

namespace TaxBeacon.API.Controllers.Accounts.Responses;

public record AccountDetailsResponse
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

    public string? Zip { get; init; }

    public string? County { get; init; }

    public string? Phone { get; init; }

    public string? Extension { get; init; }

    public string? Fax { get; init; }

    public string? Address { get; init; }

    public IEnumerable<SalesPersonResponse> SalesPersons { get; init; } = Enumerable.Empty<SalesPersonResponse>();
}
