using TaxBeacon.Common.Enums;

namespace TaxBeacon.Accounts.Accounts.Models;

public record AccountDto
{
    public Guid Id { get; init; }

    public string AccountIdField { get; init; } = null!;

    public string Name { get; init; } = null!;

    public string? City { get; init; }

    public State? State { get; init; }

    public string AccountType { get; init; } = null!;

    public string? ClientState { get; init; }

    public Status? ClientStatus { get; init; }

    public string? ClientAccountManagers { get; init; }

    public string? ReferralAccountManagers { get; init; }

    public IEnumerable<Guid> ClientAccountManagerIds { get; init; } = Enumerable.Empty<Guid>();

    public IEnumerable<Guid> ReferralAccountManagerIds { get; init; } = Enumerable.Empty<Guid>();

    public string? ReferralState { get; init; }

    public Status? ReferralStatus { get; init; }
    public string? OrganizationType { get; init; }

    public string? ReferralType { get; init; }

    public string? NaicsCode { get; init; }

    public string? NaicsCodeIndustry { get; init; }

    public string Country { get; init; } = null!;

    public string? County { get; init; }

    public int? EmployeeCount { get; init; }

    public decimal? AnnualRevenue { get; init; }

    public Guid? ClientPrimaryContactId { get; init; }

    public Guid? ReferralPrimaryContactId { get; init; }

    public string? Salespersons { get; init; }

    public IEnumerable<Guid> SalespersonIds { get; init; } = Enumerable.Empty<Guid>();

    public IEnumerable<Guid> ContactIds { get; init; } = Enumerable.Empty<Guid>();
}
