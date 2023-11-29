using TaxBeacon.Common.Enums;

namespace TaxBeacon.DAL.Accounts.Entities;

public class Entity: BaseDeletableEntity
{
    public Guid TenantId { get; set; }

    public Tenant Tenant { get; set; } = null!;

    public Guid AccountId { get; set; }

    public Account Account { get; set; } = null!;

    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string EntityId { get; set; } = null!;

    public string? DoingBusinessAs { get; set; }

    public string Country { get; set; } = null!;

    public string? Address1 { get; set; }

    public string? Address2 { get; set; }

    public string? City { get; set; }

    public State? State { get; set; }

    public string? Zip { get; set; }

    public string? County { get; set; }

    public string? Address { get; set; }

    public string Type { get; set; } = null!;

    public string? TaxYearEndType { get; set; }

    public DateTime? DateOfIncorporation { get; set; }

    public string? Fein { get; set; }

    public string? Ein { get; set; }

    public string? JurisdictionId { get; set; }

    public Status Status { get; set; }

    public DateTime? DeactivationDateTimeUtc { get; set; }

    public DateTime? ReactivationDateTimeUtc { get; set; }

    public int? PrimaryNaicsCode { get; set; }

    public NaicsCode? NaicsCode { get; set; }

    public ICollection<StateId> StateIds { get; } = new HashSet<StateId>();

    public ICollection<EntityActivityLog> EntityActivityLogs { get; set; } = new HashSet<EntityActivityLog>();

    public ICollection<EntityPhone> Phones { get; set; } = new HashSet<EntityPhone>();

    public ICollection<EntityLocation> EntityLocations { get; set; } = new HashSet<EntityLocation>();

    public ICollection<EntityDocument> EntityDocuments { get; set; } = new HashSet<EntityDocument>();
}
