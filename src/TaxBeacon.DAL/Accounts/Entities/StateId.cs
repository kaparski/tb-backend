using TaxBeacon.Common.Enums;
using TaxBeacon.DAL.Administration.Entities;

namespace TaxBeacon.DAL.Accounts.Entities;

public class StateId: BaseEntity
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid EntityId { get; set; }

    public State State { get; set; }

    public string StateIdType { get; set; } = null!;

    public string StateIdCode { get; set; } = null!;

    public string? LocalJurisdiction { get; set; }

    public Entity Entity { get; set; } = null!;

    public Tenant Tenant { get; set; } = null!;
}
