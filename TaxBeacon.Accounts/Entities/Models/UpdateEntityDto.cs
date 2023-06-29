using TaxBeacon.Common.Accounts;
using TaxBeacon.Common.Enums;
using TaxBeacon.DAL.Accounts.Entities;

namespace TaxBeacon.Accounts.Entities.Models;

public record UpdateEntityDto
(
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
    AccountEntityType Type,
    TaxYearEndType TaxYearEndType,
    Status Status,
    IEnumerable<StateId> StateIds
);
