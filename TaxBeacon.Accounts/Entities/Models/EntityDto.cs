using Mapster;
using TaxBeacon.Accounts.Common.Models;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Enums.Accounts;
using TaxBeacon.DAL.Accounts.Entities;

namespace TaxBeacon.Accounts.Entities.Models;

public record EntityDto: IRegister
{
    public Guid Id { get; init; }

    public Guid AccountId { get; init; }

    public string Name { get; init; } = null!;

    public string EntityId { get; init; } = null!;

    public string? City { get; init; }

    public string Country { get; init; } = null!;
    public DateTime? DateOfIncorporation { get; set; }

    public string? Fein { get; set; }

    public string? Ein { get; init; }

    public State? State { get; init; }

    public string? Address1 { get; init; }

    public string? Address2 { get; init; }

    public string? Zip { get; init; }

    public string? County { get; init; }

    public string? Address { get; init; }

    public string Type { get; init; } = null!;

    public string? TaxYearEndType { get; init; }

    public Status Status { get; init; }

    public DateTime CreatedDateTimeUtc { get; init; }

    public DateTime? DeactivationDateTimeUtc { get; init; }

    public DateTime? ReactivationDateTimeUtc { get; init; }

    public string? NaicsCode { get; init; }

    public string? NaicsCodeIndustry { get; init; }

    public IEnumerable<StateIdDto> StateIds { get; init; } = Enumerable.Empty<StateIdDto>();

    public IEnumerable<EntityLocationDto> EntityLocations { get; init; } = Enumerable.Empty<EntityLocationDto>();

    public IEnumerable<PhoneDto> Phones { get; init; } = Enumerable.Empty<PhoneDto>();

    public void Register(TypeAdapterConfig config) => config.NewConfig<Entity, EntityDto>()
        .Map(dest => dest.NaicsCode, src => src.PrimaryNaicsCode.ToString())
        .Map(dest => dest.NaicsCodeIndustry, src => src.NaicsCode == null ? null : src.NaicsCode.Title);
}
