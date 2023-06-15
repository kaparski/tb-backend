using TaxBeacon.Common.Enums;

namespace TaxBeacon.API.Controllers.Accounts.Responses;

public record AccountDetailsResponse(
    Guid Id,
    string Name,
    string? DoingBusinessAs,
    string? LinkedInUrl,
    string Website,
    string Country,
    string? StreetAddress1,
    string? StreetAddress2,
    string? City,
    State? State,
    int? Zip,
    string? County,
    int? Phone,
    string? Extension,
    int? Fax,
    string? Address,
    IEnumerable<SalesPersonResponse> SalesPersons);
