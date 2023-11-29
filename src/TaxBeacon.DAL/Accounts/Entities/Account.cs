using TaxBeacon.Common.Enums;

namespace TaxBeacon.DAL.Accounts.Entities;

public class Account: BaseDeletableEntity
{
    public Guid TenantId { get; set; }

    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string? DoingBusinessAs { get; set; }

    public string? LinkedInUrl { get; set; }

    public string Website { get; set; } = null!;

    public string Country { get; set; } = null!;

    public string? Address1 { get; set; }

    public string? Address2 { get; set; }

    public string? City { get; set; }

    public State? State { get; set; }

    public string? Zip { get; set; }

    public string? County { get; set; }

    public string? Address { get; set; }

    public int? PrimaryNaicsCode { get; set; }

    public string AccountId { get; set; } = null!;

    public Tenant Tenant { get; set; } = null!;

    public Client? Client { get; set; }

    public NaicsCode? NaicsCode { get; set; }

    public Referral? Referral { get; set; }

    public ICollection<Entity> Entities { get; set; } = new HashSet<Entity>();

    public ICollection<Location> Locations { get; set; } = new HashSet<Location>();

    public ICollection<AccountContact> Contacts { get; set; } = new HashSet<AccountContact>();

    public ICollection<AccountSalesperson> Salespersons { get; set; } = new HashSet<AccountSalesperson>();

    public ICollection<AccountActivityLog> AccountActivityLogs { get; set; } = new HashSet<AccountActivityLog>();

    public ICollection<AccountPhone> Phones { get; set; } = new HashSet<AccountPhone>();

    public ICollection<AccountDocument> AccountDocuments { get; set; } = new HashSet<AccountDocument>();
}
