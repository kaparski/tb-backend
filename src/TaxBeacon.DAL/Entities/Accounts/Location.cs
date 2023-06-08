using TaxBeacon.Common.Accounts;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.DAL.Entities.Accounts;
public class Location: BaseEntity
{
    public Guid TenantId { get; set; }

    public Tenant Tenant { get; set; } = null!;

    public Guid AccountId { get; set; }

    public Account Account { get; set; } = null!;

    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string? LocationId { get; set; }

    public LocationType Type { get; set; }

    public string? State { get; set; }

    public string? County { get; set; }

    public string? City { get; set; }

    public Status Status { get; set; }
}
