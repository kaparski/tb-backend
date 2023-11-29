using Mapster;
using Npoi.Mapper.Attributes;
using TaxBeacon.Accounts.Entities.Models;
using TaxBeacon.Common.Extensions;
using TaxBeacon.DAL.Accounts.Entities;

namespace TaxBeacon.Accounts.Entities.Exporters;
public class AccountEntitiesExportModel
{
    public List<EntityRow> Entities { get; set; } = new();
    public List<StateIdRow> StateIds { get; set; } = new();
    public List<LocationRow> Locations { get; set; } = new();
}

public record EntityRow: IRegister
{
    [Column("Name")]
    public string Name { get; init; } = null!;

    [Column("Entity ID")]
    public string EntityId { get; init; } = null!;

    [Column("DBA")]
    public string? DoingBusinessAs { get; init; } = string.Empty;

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

    [Column("Phone")]
    public string? PhonesView { get; set; } = string.Empty;

    [Column("Entity Type")]
    public string Type { get; init; } = null!;

    [Column("Tax Year End Type")]
    public string? TaxYearEndType { get; init; }

    [Column("Date Of Incorporation")]
    public string DateOfIncorporationView { get; set; } = null!;

    [Column("FEIN/EIN")]
    public string? FeinOrEin => Country == "United States" ? Fein : Ein;

    [Ignore]
    public string? Fein { get; set; }

    [Ignore]
    public string? Ein { get; init; }

    [Column("NAICS")]
    public string NaicsView { get; init; } = null!;

    public void Register(TypeAdapterConfig config) => config.NewConfig<EntityExportModel, EntityRow>()
            .Map(
                dest => dest.Zip,
                src => src.Zip.ApplyZipMask()
            )
            .Map(
                dest => dest.Fein,
                src => src.Fein.ApplyFeinMask()
            )
            .AfterMapping((src, dest) => dest.PhonesView = string.Join("\r\n", src.Phones.Select(p =>
                    {
                        var number = src.Country == "United States" ? p.Number.ApplyPhoneMask() : p.Number;
                        return string.IsNullOrEmpty(p.Extension)
                            ? $"{p.Type}: {number}"
                            : $"{p.Type}: {number}, {p.Extension}";
                    })));
}

public record StateIdRow
{
    [Column("Entity Name")]
    public string EntityName { get; set; } = null!;

    [Column("State (State ID)")]
    public string? State { get; init; }

    [Column("State ID Type")]
    public string? StateIdType { get; init; }

    [Column("State ID Code")]
    public string? StateIdCode { get; init; }

    [Column("Local Jurisdiction")]
    public string? LocalJurisdiction { get; init; }
}

public record LocationRow: IRegister
{
    [Column("Entity Name")]
    public string EntityName { get; set; } = null!;

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

    [Column("Phone")]
    public string? PhonesView { get; set; } = string.Empty;

    [Column("Location Type")]
    public string? Type { get; init; } = string.Empty;

    [Column("Start Date")]
    public string? StartDate { get; set; } = string.Empty;

    [Column("End Date")]
    public string? EndDate { get; set; } = string.Empty;

    [Column("NAICS")]
    public string? NaicsView { get; set; } = null!;

    public void Register(TypeAdapterConfig config) => config.NewConfig<Location, LocationRow>()
            .Map(
                dest => dest.NaicsView,
                src => src.NaicsCode != null ? src.NaicsCode.Code + " " + src.NaicsCode.Title : null
            )
            .Map(
                dest => dest.Zip,
                src => src.Zip.ApplyZipMask()
            )
            .AfterMapping((src, dest) => dest.PhonesView = string.Join("\r\n", src.Phones.Select(p =>
            {
                var number = src.Country == "United States" ? p.Number.ApplyPhoneMask() : p.Number;
                return string.IsNullOrEmpty(p.Extension)
                    ? $"{p.Type}: {number}"
                    : $"{p.Type}: {number}, {p.Extension}";
            })));
}
