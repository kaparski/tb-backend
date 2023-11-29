using TaxBeacon.Common.Enums;

namespace TaxBeacon.DAL.Accounts.Entities;

public class AccountView: BaseDeletableEntity
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string Name { get; set; } = null!;

    public string AccountId { get; set; } = null!;

    public string? DoingBusinessAs { get; set; }

    public State? State { get; set; }

    public string? City { get; set; }

    public string? Address { get; set; }

    public string? Address1 { get; set; }

    public string? Address2 { get; set; }

    public string? Zip { get; set; }

    public string Website { get; set; } = null!;

    public string? LinkedInUrl { get; set; }

    public int? FoundationYear { get; set; }

    public string AccountType { get; set; } = null!;

    public string? ClientState { get; set; }

    public Status? ClientStatus { get; set; }

    public string? ReferralState { get; set; }

    public Status? ReferralStatus { get; set; }

    public string? OrganizationType { get; set; }

    public string? ReferralType { get; set; }

    public string? ClientAccountManagers { get; set; }

    public string? ReferralAccountManagers { get; set; }

    public string? Salespersons { get; set; }

    public string? ClientPrimaryContact { get; set; }

    public string? ReferralPrimaryContact { get; set; }

    public string? NaicsCode { get; set; }

    public string? NaicsCodeIndustry { get; set; }

    public string Country { get; set; } = null!;

    public string? County { get; set; }

    public int? EmployeeCount { get; set; }

    public decimal? AnnualRevenue { get; set; }

    public Guid? ClientPrimaryContactId { get; set; }

    public Guid? ReferralPrimaryContactId { get; set; }
}
