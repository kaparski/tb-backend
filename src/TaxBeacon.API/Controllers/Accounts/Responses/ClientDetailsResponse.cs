using TaxBeacon.Accounts.Accounts.Models;
using TaxBeacon.Accounts.Services.Contacts.Models;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.API.Controllers.Accounts.Responses;

public record ClientDetailsResponse(
    string State,
    Status Status,
    decimal? AnnualRevenue,
    int? FoundationYear,
    int? EmployeeCount,
    DateTime? DeactivationDateTimeUtc,
    DateTime? ReactivationDateTimeUtc,
    DateTime CreatedDateTimeUtc,
    ContactDto? PrimaryContact,
    ICollection<ClientManagerDto> ClientManagers
    );
