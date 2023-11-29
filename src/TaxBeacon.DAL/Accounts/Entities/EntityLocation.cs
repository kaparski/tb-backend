using NPOI.SS.Formula.Functions;
using TaxBeacon.DAL.Administration.Entities;

namespace TaxBeacon.DAL.Accounts.Entities;

public class EntityLocation
{
    public Guid TenantId { get; set; }

    public Guid EntityId { get; set; }

    public Guid LocationId { get; set; }

    public Entity Entity { get; set; } = null!;

    public Location Location { get; set; } = null!;

    public Tenant Tenant { get; set; } = null!;
}
