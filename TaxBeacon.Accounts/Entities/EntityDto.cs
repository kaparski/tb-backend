using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Accounts;

namespace TaxBeacon.Accounts.Entities;
public record EntityDto
{
    public Guid Id { get; init; }

    public string Name { get; init; } = null!;

    public string? EntityId { get; init; }

    public string? City { get; init; }

    public int Fein { get; set; }

    public State State { get; init; }

    public AccountEntityType Type { get; init; } = AccountEntityType.None;

    public Status Status { get; init; }
}
