namespace TaxBeacon.Accounts.Accounts.Models;

public sealed record CreateClientDto
{
    public decimal? AnnualRevenue { get; init; }

    public int? FoundationYear { get; init; }

    public int? EmployeeCount { get; init; }

    public IEnumerable<Guid> ClientManagersIds { get; init; } = Enumerable.Empty<Guid>();
}
