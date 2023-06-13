using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Accounts;

namespace TaxBeacon.Accounts.Entities;
public class EntityDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string? EntityId { get; set; }

    public string? City { get; set; }

    public State State { get; set; }

    public AccountEntityType Type { get; set; } = AccountEntityType.None;

    public Status Status { get; set; }
}
