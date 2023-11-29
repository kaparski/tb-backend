using TaxBeacon.Accounts.Entities.Models;
using TaxBeacon.API.Controllers.StateIds.Responses;
using TaxBeacon.API.Shared.Responses;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.API.Controllers.Entities.Responses;

public record EntityResponse
{
    public Guid Id { get; init; }

    public Guid AccountId { get; init; }

    public string EntityId { get; init; } = null!;

    public string Name { get; init; } = null!;

    public string? City { get; init; }

    public string Country { get; init; } = null!;

    public DateTime? DateOfIncorporation { get; init; }

    public string? Fein { get; init; }

    public string? Ein { get; init; }

    public string? Address1 { get; init; }

    public string? Address2 { get; init; }

    public string? Zip { get; init; }

    public string? County { get; init; }

    public string? Address { get; init; }

    public State? State { get; init; }

    public string Type { get; init; } = null!;

    public string? TaxYearEndType { get; init; }

    public Status Status { get; init; }

    public string? NaicsCode { get; init; }

    public string? NaicsCodeIndustry { get; init; }

    public DateTime CreatedDateTimeUtc { get; init; }

    public DateTime? DeactivationDateTimeUtc { get; init; }

    public DateTime? ReactivationDateTimeUtc { get; init; }

    public IEnumerable<StateIdDto> StateIds { get; init; } = Enumerable.Empty<StateIdDto>();

    public IEnumerable<EntityLocationResponse> EntityLocations { get; init; } = Enumerable.Empty<EntityLocationResponse>();

    public IEnumerable<PhoneResponse> Phones { get; init; } = Enumerable.Empty<PhoneResponse>();
}
