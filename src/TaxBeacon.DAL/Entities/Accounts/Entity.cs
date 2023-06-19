using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Accounts;

namespace TaxBeacon.DAL.Entities.Accounts;
public class Entity: BaseEntity
{
    public Guid TenantId { get; set; }

    public Tenant Tenant { get; set; } = null!;

    public Guid AccountId { get; set; }

    public Account Account { get; set; } = null!;

    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string? EntityId { get; set; }

    public string? City { get; set; }

    public int Fein { get; set; }

    public State State { get; set; }

    public AccountEntityType Type { get; set; } = AccountEntityType.None;

    public Status Status { get; set; }
}
