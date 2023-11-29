using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Enums.Accounts;

namespace TaxBeacon.DAL.Accounts.Entities;

public class Location: BaseDeletableEntity
{
    public Guid TenantId { get; set; }

    public Guid AccountId { get; set; }

    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string LocationId { get; set; } = null!;

    public LocationType Type { get; set; }

    public string Country { get; set; } = null!;

    public string? Address1 { get; set; }

    public string? Address2 { get; set; }

    public string? City { get; set; }

    public string? County { get; set; }

    public State? State { get; set; }

    public string? Zip { get; set; }

    public string? Address { get; set; }

    public int? PrimaryNaicsCode { get; set; }

    public NaicsCode? NaicsCode { get; set; }

    public Status Status { get; set; }

    public DateTime? DeactivationDateTimeUtc { get; set; }

    public DateTime? ReactivationDateTimeUtc { get; set; }

    public DateTime? StartDateTimeUtc { get; set; }

    public DateTime? EndDateTimeUtc { get; set; }

    public Tenant Tenant { get; set; } = null!;

    public Account Account { get; set; } = null!;

    public ICollection<LocationPhone> Phones { get; set; } = new HashSet<LocationPhone>();

    public ICollection<EntityLocation> EntityLocations { get; set; } = new HashSet<EntityLocation>();

    public ICollection<LocationActivityLog> LocationActivityLogs { get; set; } = new HashSet<LocationActivityLog>();

    public ICollection<LocationDocument> LocationDocuments { get; set; } = new HashSet<LocationDocument>();
}
