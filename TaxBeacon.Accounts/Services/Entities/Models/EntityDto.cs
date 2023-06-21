using TaxBeacon.Common.Enums;
using TaxBeacon.DAL.Entities.Accounts;
using Mapster;

namespace TaxBeacon.Accounts.Services.Entities.Models;
public record EntityDto
{
    public Guid Id { get; init; }

    public string Name { get; init; } = null!;

    public string? EntityId { get; init; }

    public string? City { get; init; }

    public int Fein { get; set; }

    public State State { get; init; }

    public string Type { get; init; } = null!;

    public Status Status { get; init; }
}
