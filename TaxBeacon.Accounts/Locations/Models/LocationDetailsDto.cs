using TaxBeacon.Accounts.Common.Models;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Enums.Accounts;

namespace TaxBeacon.Accounts.Locations.Models;

public record LocationDetailsDto
{
    public Guid Id { get; init; }

    public string Name { get; init; } = null!;

    public string LocationId { get; init; } = null!;

    public LocationType Type { get; init; }

    public string Country { get; init; } = null!;

    public string? Address1 { get; init; }

    public string? Address2 { get; init; }

    public string? City { get; init; }

    public string? County { get; init; }

    public State? State { get; init; }

    public string? Zip { get; init; }

    public string? Address { get; init; }

    public NaicsCodeDto? NaicsCode { get; init; }

    public Status Status { get; init; }

    public DateTime CreatedDateTimeUtc { get; init; }

    public DateTime? StartDateTimeUtc { get; init; }

    public DateTime? EndDateTimeUtc { get; init; }

    public DateTime? DeactivationDateTimeUtc { get; init; }

    public DateTime? ReactivationDateTimeUtc { get; init; }

    public DateTime? LastModifiedDateTimeUtc { get; init; }

    public IEnumerable<PhoneDto> Phones { get; init; } = Enumerable.Empty<PhoneDto>();
}
