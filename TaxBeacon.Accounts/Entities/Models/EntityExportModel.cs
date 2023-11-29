using Mapster;
using Npoi.Mapper.Attributes;
using TaxBeacon.Accounts.Common.Models;
using TaxBeacon.DAL.Accounts.Entities;

namespace TaxBeacon.Accounts.Entities.Models;

public record EntityExportModel: IRegister
{
    public string Name { get; init; } = null!;

    public string EntityId { get; init; } = null!;

    public string? DoingBusinessAs { get; init; } = string.Empty;

    public string Country { get; init; } = null!;

    public string? Address1 { get; init; } = string.Empty;

    public string? Address2 { get; init; } = string.Empty;

    public string? City { get; init; } = string.Empty;

    public string? State { get; init; } = string.Empty;

    public string? County { get; init; } = string.Empty;

    public string? Zip { get; set; } = string.Empty;

    public string? Address { get; init; } = string.Empty;

    public PhoneDto[] Phones { get; init; } = { };

    public string Type { get; init; } = null!;

    public string? TaxYearEndType { get; init; }

    public DateTime? DateOfIncorporation { get; init; }

    public string? Fein { get; init; } = string.Empty;

    public string? Ein { get; init; } = string.Empty;

    public string NaicsView { get; init; } = null!;

    public StateIdDto[] StateIds { get; set; } = { };

    public Location[] Locations { get; set; } = { };

    public void Register(TypeAdapterConfig config) =>
        config.NewConfig<Entity, EntityExportModel>()
            .Map(
                dest => dest.StateIds,
                src => src.StateIds.OrderBy(x => x.State).ToArray()
            )
            .Map(
                dest => dest.Locations,
                src => src.EntityLocations.Select(x => x.Location).OrderBy(x => x.State).ToArray()
            )
            .Map(x => x.NaicsView, src => src.NaicsCode != null ? src.NaicsCode.Code + " " + src.NaicsCode.Title : null);
}

