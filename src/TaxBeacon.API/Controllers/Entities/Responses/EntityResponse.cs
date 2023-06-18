using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Accounts;

namespace TaxBeacon.API.Controllers.Entities.Responses;

public record EntityResponse
{
    public Guid Id { get; init; }

    public string Name { get; init; } = null!;

    public string? EntityId { get; init; }

    public string? City { get; init; }

    public State State { get; init; }

    public AccountEntityType Type { get; init; } = AccountEntityType.None;

    public Status Status { get; init; }
}
