using TaxBeacon.Common.Accounts;
using TaxBeacon.Common.Enums;
using TaxBeacon.DAL.Accounts.Entities;

namespace TaxBeacon.API.Controllers.Entities.Responses;

public record EntityDetailsResponse
(
    Guid Id,
    string Name,
    string? Dba,
    string? EntityId,
    string? City,
    string StreetAddress1,
    string? StreetAddress2,
    string? Address,
    int Fein,
    int? Zip,
    string Country,
    string? Fax,
    string? Phone,
    string? Extension,
    State State,
    string Type,
    TaxYearEndType TaxYearEndType,
    Status Status,
    IEnumerable<StateId> StateIds
);
