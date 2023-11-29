using Mapster;
using Npoi.Mapper.Attributes;
using TaxBeacon.Accounts.Common.Models;
using TaxBeacon.DAL.Accounts.Entities;

namespace TaxBeacon.Accounts.Locations.Models;

public class LocationExportModel: IRegister
{
    [Column("Location Name")]
    public string Name { get; init; } = null!;

    [Column("Location ID")]
    public string LocationId { get; init; } = null!;

    [Column("Country")]
    public string Country { get; init; } = null!;

    [Column("Address 1")]
    public string? Address1 { get; init; } = string.Empty;

    [Column("Address 2")]
    public string? Address2 { get; init; } = string.Empty;

    [Column("City")]
    public string? City { get; init; } = string.Empty;

    [Column("County")]
    public string? County { get; init; } = string.Empty;

    [Column("State")]
    public string? State { get; init; } = string.Empty;

    [Column("Zip")]
    public string? Zip { get; set; } = string.Empty;

    [Column("Address")]
    public string? Address { get; init; } = string.Empty;

    [Ignore]
    public PhoneDto[] Phones { get; init; } = { };

    [Column("Phone")]
    public string? PhonesView { get; set; } = string.Empty;

    [Column("Location type")]
    public string Type { get; init; } = null!;

    [Ignore]
    public DateTime? StartDateTimeUtc { get; set; }

    [Column("Start Date")]
    public string? StartDateView { get; set; } = string.Empty;

    [Ignore]
    public DateTime? EndDateTimeUtc { get; set; }

    [Column("End Date")]
    public string? EndDateView { get; set; } = string.Empty;

    [Column("NAICS")]
    public string? PrimaryNaics { get; init; }

    [Column("Associated Entities")]
    public string? EntitiesView { get; init; } = string.Empty;

    public void Register(TypeAdapterConfig config) =>
        config.NewConfig<Location, LocationExportModel>()
            .Map(
                dest => dest.PrimaryNaics,
                src => src.PrimaryNaicsCode == null ? string.Empty : $"{src.PrimaryNaicsCode} {src.NaicsCode!.Title}"
            )
            .Map(
                dest => dest.EntitiesView,
                src => string.Join(", ", src.EntityLocations
                    .Select(e => e.Entity.Name)
                    .OrderBy(e => e))
            );

}
