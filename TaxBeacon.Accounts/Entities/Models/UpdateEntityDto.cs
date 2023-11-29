using Mapster;
using TaxBeacon.Accounts.Common.Models;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Enums.Accounts;
using TaxBeacon.DAL.Accounts.Entities;

namespace TaxBeacon.Accounts.Entities.Models;

public record UpdateEntityDto: IRegister, IValidateEntityModel
{
    public string Name { get; init; } = null!;

    public string EntityId { get; init; } = null!;

    public string? DoingBusinessAs { get; init; }

    public Country Country { get; init; } = null!;

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

    public AccountEntityType Type { get; init; } = null!;

    public TaxYearEndType? TaxYearEndType { get; init; }

    public DateTime? DateOfIncorporation { get; init; }

    public int? PrimaryNaicsCode { get; init; }

    public IEnumerable<CreateUpdatePhoneDto> Phones { get; init; } = Enumerable.Empty<CreateUpdatePhoneDto>();

    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<UpdateEntityDto, Entity>()
            .Map(dest => dest.Country, src => src.Country.Name)
            .Map(dest => dest.Type, src => src.Type.Name)
            .Map(dest => dest.TaxYearEndType, src => src.TaxYearEndType == null ? null : src.TaxYearEndType.Name)
            .Ignore(x => x.Phones);

        config.NewConfig<Entity, UpdateEntityDto>()
            .Map(dest => dest.Country, src => Country.FromName(src.Country, true))
            .Map(dest => dest.Type, src => AccountEntityType.FromName(src.Type, true))
            .Map(dest => dest.TaxYearEndType, src => src.TaxYearEndType == null ? null : TaxYearEndType.FromName(src.TaxYearEndType, true));
    }
}
