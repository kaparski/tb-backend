using TaxBeacon.Common.Enums;
using TaxBeacon.DAL.Administration.Entities;

namespace TaxBeacon.DAL.Accounts.Entities;

public class Account: BaseEntity
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

    public Tenant Tenant { get; set; } = null!;

    public Client? Client { get; set; }

    public Referral? Referral { get; set; }

    public ICollection<Entity> Entities { get; set; } = new HashSet<Entity>();

    public ICollection<Location> Locations { get; set; } = new HashSet<Location>();

    public ICollection<Contact> Contacts { get; set; } = new HashSet<Contact>();

    public ICollection<AccountSalesperson> Salespersons { get; set; } = new HashSet<AccountSalesperson>();

    public ICollection<AccountActivityLog> AccountActivityLogs { get; set; } = new HashSet<AccountActivityLog>();

    public ICollection<Phone> Phones { get; set; } = new HashSet<Phone>();
}
