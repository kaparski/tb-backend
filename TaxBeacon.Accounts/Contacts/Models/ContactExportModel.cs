using Npoi.Mapper.Attributes;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.Accounts.Contacts.Models;

public class ContactExportModel
{
    [Column("Name")]
    public string FullName { get; set; } = string.Empty;

    [Column("Email")]
    public string Email { get; set; } = string.Empty;

    [Column("Job Title")]
    public string JobTitle { get; set; } = string.Empty;

    [Column("Contact Type")]
    public string ContactType { get; set; } = string.Empty;

    [Column("Role")]
    public string? Role { get; set; }

    [Column("Additional Role")]
    public string? SubRole { get; set; }

    [Column("Country")]
    public string? Country { get; set; }

    [Column("Street Address 1")]
    public string? Address { get; set; }

    [Column("City")]
    public string? City { get; set; }

    [Column("State")]
    public State State { get; set; } = State.None;

    [Column("Zip")]
    public string? Zip { get; set; }

    [Column("Phone")]
    public string Phone { get; set; } = null!;

    [Column("Phone 2")]
    public string? Phone2 { get; set; }
}
