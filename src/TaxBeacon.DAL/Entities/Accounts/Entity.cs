using TaxBeacon.Common.Enums;
using EntityType = TaxBeacon.Common.Accounts.EntityType;

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

    public string? State { get; set; }

    public EntityType Type { get; set; } = EntityType.None;

    public Status Status { get; set; }
}
