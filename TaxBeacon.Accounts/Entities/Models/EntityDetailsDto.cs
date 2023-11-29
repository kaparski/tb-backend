using TaxBeacon.Accounts.Common.Models;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Enums.Accounts;
using TaxBeacon.DAL.Accounts.Entities;

namespace TaxBeacon.Accounts.Entities.Models;

public record EntityDetailsDto
{
    public Guid Id { get; init; }

    public Guid AccountId { get; init; }

    public string EntityId { get; init; } = null!;

    public string Name { get; init; } = null!;

    public string? DoingBusinessAs { get; init; }

    public string Country { get; init; } = null!;

    public string? Address1 { get; init; }

    public string? Address2 { get; init; }

    public string? City { get; init; }

    public State? State { get; init; }

    public string? Zip { get; init; }

    public string? County { get; init; }

    public string? Address { get; init; }

    public string? Fein { get; init; }

    public string? Ein { get; init; }

    public string? JurisdictionId { get; init; }

    public string Type { get; init; } = null!;

    public NaicsCodeDto? NaicsCode { get; init; }

    public DateTime CreatedDateTimeUtc { get; init; }

    public DateTime? LastModifiedDateTimeUtc { get; init; }

    public DateTime? DeactivationDateTimeUtc { get; init; }

    public DateTime? ReactivationDateTimeUtc { get; init; }

    public string TaxYearEndType { get; init; } = null!;

    public DateTime? DateOfIncorporation { get; init; }

    public Status Status { get; init; }

    public IEnumerable<StateIdDto> StateIds { get; init; } = Enumerable.Empty<StateIdDto>();

    public IEnumerable<PhoneDto> Phones { get; init; } = Enumerable.Empty<PhoneDto>();
}
