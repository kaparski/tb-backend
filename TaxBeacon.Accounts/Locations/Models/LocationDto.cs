using Mapster;
using TaxBeacon.Accounts.Entities.Models;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Enums.Accounts;
using TaxBeacon.DAL.Accounts.Entities;

namespace TaxBeacon.Accounts.Locations.Models;

public record LocationDto: IRegister
{
    public Guid Id { get; init; }

    public Guid AccountId { get; init; }

    public string LocationId { get; init; } = null!;

    public string Name { get; init; } = null!;

    public LocationType Type { get; init; }

    public State? State { get; init; }

    public string? County { get; init; }

    public string? City { get; init; }

    public Status Status { get; init; }

    public string Country { get; init; } = null!;

    public IEnumerable<EntityLocationDto> EntityLocations { get; init; } = Enumerable.Empty<EntityLocationDto>();

    public DateTime? StartDateTimeUtc { get; init; }

    public DateTime? EndDateTimeUtc { get; init; }

    public DateTime CreatedDateTimeUtc { get; init; }

    public DateTime? ReactivationDateTimeUtc { get; init; }

    public DateTime? DeactivationDateTimeUtc { get; init; }

    public string? NaicsCode { get; init; }

    public string? NaicsCodeIndustry { get; init; }

    public void Register(TypeAdapterConfig config) => config.NewConfig<Location, LocationDto>()
        .Map(l => l.NaicsCodeIndustry, l => l.NaicsCode == null ? null : l.NaicsCode.Title)
        .Map(l => l.NaicsCode, l => l.PrimaryNaicsCode.ToString());
}
