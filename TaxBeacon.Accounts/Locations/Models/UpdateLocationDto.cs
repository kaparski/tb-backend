using Mapster;
using TaxBeacon.Accounts.Common.Models;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Enums.Accounts;
using TaxBeacon.DAL.Accounts.Entities;

namespace TaxBeacon.Accounts.Locations.Models;

public record UpdateLocationDto: IRegister
{
    public string Name { get; init; } = null!;

    public Country Country { get; init; } = null!;

    public string? Address1 { get; init; }

    public string? Address2 { get; init; }

    public string? City { get; init; }

    public State? State { get; init; }

    public string? Zip { get; init; }

    public string? County { get; init; }

    public string? Address { get; init; }

    public string LocationId { get; init; } = null!;

    public int? PrimaryNaicsCode { get; init; }

    public DateTime? StartDateTimeUtc { get; init; }

    public DateTime? EndDateTimeUtc { get; init; }

    public LocationType Type { get; init; }

    public IEnumerable<CreateUpdatePhoneDto> Phones { get; init; } = Enumerable.Empty<CreateUpdatePhoneDto>();

    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<UpdateLocationDto, Location>()
            .Map(dest => dest.Country, src => src.Country.Name)
            .Ignore(x => x.Phones);

        config.NewConfig<Location, UpdateLocationDto>()
            .Map(dest => dest.Country, src => Country.FromName(src.Country, true));
    }
}
