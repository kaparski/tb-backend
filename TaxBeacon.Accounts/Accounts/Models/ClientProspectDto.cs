using TaxBeacon.Common.Enums;

namespace TaxBeacon.Accounts.Accounts.Models;

public record ClientProspectDto
{
    public Guid AccountId { get; init; }

    public string AccountIdField { get; init; } = null!;

    public string Name { get; init; } = null!;

    public string? City { get; init; }

    public int DaysOpen { get; init; }

    public State? State { get; init; }

    public Status Status { get; init; }

    public string? Salespersons { get; init; } = null!;

    public IEnumerable<Guid> SalespersonIds { get; internal set; } = Enumerable.Empty<Guid>();

    public DateTime CreatedDateTimeUtc { get; init; }

    public DateTime? DeactivationDateTimeUtc { get; init; }

    public DateTime? ReactivationDateTimeUtc { get; init; }

    public string? NaicsCode { get; init; }

    public string? NaicsCodeIndustry { get; init; }

    public string Country { get; init; } = null!;

    public string? County { get; init; }

    public int? EmployeeCount { get; init; }

    public decimal? AnnualRevenue { get; init; }

    public Guid? PrimaryContactId { get; init; }

    public IEnumerable<Guid> AccountManagerIds { get; init; } = Enumerable.Empty<Guid>();
}
