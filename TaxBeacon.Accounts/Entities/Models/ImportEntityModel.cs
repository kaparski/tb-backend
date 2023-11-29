using CsvHelper.Configuration.Attributes;
using Mapster;
using TaxBeacon.Accounts.Common;
using TaxBeacon.Common.Extensions;
using TaxBeacon.DAL.Accounts.Entities;

namespace TaxBeacon.Accounts.Entities.Models;

public record ImportEntityModel: IRegister
{
    [Name("Entity Name*")]
    public string Name { get; init; } = null!;

    [Name("Entity ID")]
    public string? EntityId { get; init; }

    [Name("DBA")]
    public string? DoingBusinessAs { get; init; }

    [Name("Country*")]
    public string Country { get; init; } = null!;

    [Name("Address 1**")]
    public string? Address1 { get; init; }

    [Name("Address 2")]
    public string? Address2 { get; init; }

    [Name("City**")]
    public string? City { get; init; }

    [Name("State**")]
    public string? State { get; init; }

    [Name("County**")]
    public string? County { get; init; }

    [Name("Zip**")]
    public string? Zip { get; set; }

    [Name("Address***")]
    public string? Address { get; init; }

    public string? Phone { get; init; }

    [Name("Entity Type*")]
    public string Type { get; init; } = null!;

    [Name("Tax Year End Type")]
    public string? TaxYearEndType { get; init; }

    [Name("Date Of Incorporation")]
    public DateTime? DateOfIncorporation { get; init; }

    [Name("FEIN")]
    public string? Fein { get; init; }

    [Name("EIN")]
    public string? Ein { get; init; }

    [Name("NAICS Code")]
    public int? PrimaryNaicsCode { get; init; }

    public void Register(TypeAdapterConfig config) =>
        config.NewConfig<ImportEntityModel, Entity>()
            .IgnoreIf((src, dest) =>
                string.IsNullOrEmpty(src.EntityId), dest => dest.EntityId)
            .IgnoreIf((src, dest) =>
                string.IsNullOrEmpty(src.DoingBusinessAs), dest => dest.DoingBusinessAs)
            .IgnoreIf((src, dest) =>
                string.IsNullOrEmpty(src.Address1), dest => dest.Address1)
            .IgnoreIf((src, dest) =>
                string.IsNullOrEmpty(src.Address2), dest => dest.Address2)
            .IgnoreIf((src, dest) =>
                string.IsNullOrEmpty(src.City), dest => dest.City)
            .IgnoreIf((src, dest) =>
                string.IsNullOrEmpty(src.Zip), dest => dest.Zip)
            .IgnoreIf((src, dest) =>
                string.IsNullOrEmpty(src.County), dest => dest.County)
            .IgnoreIf((src, dest) =>
                string.IsNullOrEmpty(src.State), dest => dest.State)
            .IgnoreIf((src, dest) =>
                string.IsNullOrEmpty(src.Address), dest => dest.Address)
            .IgnoreIf((src, dest) =>
                string.IsNullOrEmpty(src.TaxYearEndType), dest => dest.TaxYearEndType)
            .IgnoreIf((src, dest) =>
                string.IsNullOrEmpty(src.Fein), dest => dest.Fein)
            .IgnoreIf((src, dest) =>
                string.IsNullOrEmpty(src.Ein), dest => dest.Ein)
            .Map(dest => dest.Phones,
                src => Helpers.GetPhonesFromString<EntityPhone>(src.Phone))
            .Map(dest => dest.Zip,
                src => src.Country == "United States" ? src.Zip.RemoveZipMask() : src.Zip)
            .Map(dest => dest.Fein,
                src => src.Country == "United States" ? src.Fein.RemoveFeinMask() : src.Fein);
}
