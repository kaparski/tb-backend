using Mapster;
using Npoi.Mapper.Attributes;
using TaxBeacon.Administration.Programs.Models;
using TaxBeacon.Common.Enums;
using TaxBeacon.DAL.Accounts.Entities;
using TaxBeacon.DAL.Administration.Entities;

namespace TaxBeacon.Accounts.Accounts.Models;
public record ReferralPartnerExportDto: IRegister
{
    [Ignore]
    public Guid Id { get; set; }

    [Column("Account Name")]
    public string Name { get; init; } = null!;

    [Column("Account ID")]
    public string AccountId { get; init; } = null!;

    [Column("DBA")]
    public string? DoingBusinessAs { get; init; }

    [Column("Country")]
    public string? Country { get; init; }

    [Column("Address 1")]
    public string? Address1 { get; init; }

    [Column("Address 2")]
    public string? Address2 { get; init; }

    [Column("City")]
    public string? City { get; init; }

    [Column("County")]
    public string? County { get; init; }

    [Column("State")]
    public State? State { get; init; }

    [Column("Zip")]
    public string? Zip { get; set; }

    [Column("Address")]
    public string? Address { get; init; }

    [Column("Phone")]
    public string? Phones { get; set; }

    [Column("Linkedin URL")]
    public string? LinkedInUrl { get; init; }

    [Column("Website")]
    public string Website { get; init; } = string.Empty;

    [Column("Salesperson")]
    public string Salespersons { get; init; } = string.Empty;

    [Column("NAICS")]
    public string Naics { get; init; } = string.Empty;

    [Column("Organization Type")]
    public string? OrganizationType { get; init; }

    [Column("Referral Type")]
    public string? ReferralType { get; init; }

    [Column("Referral Primary Contact")]
    public string? ReferralPrimaryContact { get; init; }

    [Column("Referral Account Manager")]
    public string? ReferralManagers { get; init; }

    [Column("Status")]
    public Status? ReferralStatus { get; init; }

    public void Register(TypeAdapterConfig config) =>
    config.NewConfig<AccountView, ReferralPartnerExportDto>().Map(
                dest => dest.Naics,
                src => src.NaicsCode != null ? src.NaicsCode + " " + src.NaicsCodeIndustry : null
            );
}
