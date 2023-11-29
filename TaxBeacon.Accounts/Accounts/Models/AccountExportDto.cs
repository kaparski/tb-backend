﻿using Mapster;
using Npoi.Mapper.Attributes;
using TaxBeacon.Common.Enums;
using TaxBeacon.DAL.Accounts.Entities;

namespace TaxBeacon.Accounts.Accounts.Models;

public record AccountExportDto: IRegister
{
    [Ignore]
    public Guid Id { get; set; }

    [Column("Account Name")]
    public string Name { get; init; } = null!;

    [Column("Account Type")]
    public string AccountType { get; init; } = null!;

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
    public string? Naics { get; init; }

    [Column("Employee Count")]
    public int? EmployeeCount { get; init; }

    [Ignore]
    public decimal? AnnualRevenue { get; init; }

    [Column("Annual Revenue")]
    public string? AnnualRevenueView { get; set; }

    [Column("Founded Year")]
    public int? FoundationYear { get; init; }

    [Column("Client Account Manager")]
    public string? ClientAccountManagers { get; init; }

    [Column("Client Primary Contact")]
    public string? ClientPrimaryContact { get; init; }

    [Column("Organization Type")]
    public string? OrganizationType { get; init; }

    [Column("Referral Type")]
    public string? ReferralType { get; init; }

    [Column("Referral Account Manager")]
    public string? ReferralAccountManagers { get; init; }

    [Column("Referral Primary Contact")]
    public string? ReferralPrimaryContact { get; init; }

    public void Register(TypeAdapterConfig config) =>
        config.NewConfig<AccountView, AccountExportDto>().Map(
            dest => dest.Naics,
            src => src.NaicsCode != null ? src.NaicsCode + " " + src.NaicsCodeIndustry : null
        );
}
