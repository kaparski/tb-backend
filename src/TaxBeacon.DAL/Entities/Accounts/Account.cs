namespace TaxBeacon.DAL.Entities.Accounts;

public class Account: BaseEntity
{
    public Guid TenantId { get; set; }

    public Tenant Tenant { get; set; } = null!;

    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string? State { get; set; }

    public string? City { get; set; }

    public string? Address1 { get; set; }

    public string? Address2 { get; set; }

    public string? DoingBusinessAs { get; set; }

    public string? LinkedInURL { get; set; }

    public string? Website { get; set; }

    public string? Phone { get; set; }

    public string? Extension { get; set; }

    public string? Fax { get; set; }

    public string? Country { get; set; }

    public string? County { get; set; }

    public ICollection<AccountType> AccountTypes { get; set; } = new HashSet<AccountType>();
}
