namespace TaxBeacon.Accounts.Accounts.Models;
public sealed record UpdateClientDto(
    decimal? AnnualRevenue,
    int? FoundationYear,
    int? EmployeeCount,
    Guid? PrimaryContactId,
    IEnumerable<Guid> ClientManagersIds
);
