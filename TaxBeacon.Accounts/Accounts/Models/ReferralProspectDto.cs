using TaxBeacon.Common.Enums;

namespace TaxBeacon.Accounts.Accounts.Models;
public record ReferralProspectDto
{
    public Guid Id { get; init; }

    public string AccountIdField { get; init; } = null!;

    public string Name { get; init; } = null!;

    public string? City { get; init; }

    public State? State { get; init; }

    public string? PrimaryContactName { get; init; }

    public Status Status { get; init; }

    public string? AccountManagers { get; init; }

    public string? NaicsCode { get; init; }

    public string? NaicsCodeIndustry { get; init; }

    public DateTime CreatedDateTimeUtc { get; init; }

    public DateTime? DeactivationDateTimeUtc { get; init; }

    public DateTime? ReactivationDateTimeUtc { get; init; }

    public string Country { get; init; } = null!;

    public string? County { get; init; }

    public string? OrganizationType { get; init; }

    public string? Type { get; init; }

    public int DaysOpen { get; set; }

    public Guid? PrimaryContactId { get; init; }

    public string? Salespersons { get; init; }

    public IEnumerable<Guid> SalespersonIds { get; init; } = Enumerable.Empty<Guid>();

    public IEnumerable<Guid> AccountManagerIds { get; init; } = Enumerable.Empty<Guid>();
}
