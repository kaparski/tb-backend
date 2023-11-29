using TaxBeacon.API.Shared.Responses;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Enums.Accounts;

namespace TaxBeacon.API.Controllers.Entities.Responses;

public record EntityDetailsResponse(
    Guid Id,
    Guid AccountId,
    string Name,
    string EntityId,
    string? DoingBusinessAs,
    string Country,
    string? Address1,
    string? Address2,
    string? City,
    State? State,
    string? Zip,
    string? County,
    string? Address,
    string? Fein,
    string? Ein,
    string? JurisdictionId,
    string Type,
    NaicsCodeResponse? NaicsCode,
    DateTime? LastModifiedDateTimeUtc,
    DateTime? DeactivationDateTimeUtc,
    DateTime? ReactivationDateTimeUtc,
    DateTime CreatedDateTimeUtc,
    string TaxYearEndType,
    DateTime? DateOfIncorporation,
    Status Status,
    IEnumerable<PhoneResponse> Phones
    );
