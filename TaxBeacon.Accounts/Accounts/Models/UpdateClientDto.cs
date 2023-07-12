namespace TaxBeacon.Accounts.Accounts.Models;
public sealed record UpdateClientDto()
{
    public decimal? AnnualRevenue { get; init; }
    public int? FoundationYear { get; init; }
    public int? EmployeeCount { get; init; }
    public Guid? PrimaryContactId { get; init; }
    public IEnumerable<Guid> ClientManagersIds { get; init; } = Enumerable.Empty<Guid>();
};
