using TaxBeacon.Common.Enums;

namespace TaxBeacon.DAL.Accounts.Entities;
public class ReferralView: BaseDeletableEntity
{
    public Guid TenantId { get; set; }

    public Guid AccountId { get; set; }

    public string AccountIdField { get; set; } = null!;

    public State? State { get; set; }

    public string Name { get; set; } = null!;

    public string? City { get; set; }

    public string? PrimaryContactName { get; set; }

    public string? ReferralState { get; set; }

    public string? Type { get; set; }

    public string? OrganizationType { get; set; }

    public Status Status { get; set; }

    public int DaysOpen { get; set; }

    public string? Salespersons { get; set; }

    public string? AccountManagers { get; set; }

    public string? NaicsCode { get; set; }

    public string? NaicsCodeIndustry { get; set; }

    public DateTime? DeactivationDateTimeUtc { get; set; }

    public DateTime? ReactivationDateTimeUtc { get; set; }

    public string Country { get; set; } = null!;

    public string? County { get; set; }

    public Guid? PrimaryContactId { get; set; }
}
