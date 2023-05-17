using Npoi.Mapper.Attributes;

namespace TaxBeacon.UserManagement.Models.Programs;

public class ProgramExportModel
{
    public string Reference { get; set; } = string.Empty;

    [Column("Program Name")]
    public string Name { get; set; } = string.Empty;

    [Column("Program Overview")]
    public string Overview { get; set; } = string.Empty;

    [Column("Legal Authority")]
    public string LegalAuthority { get; set; } = string.Empty;

    [Column("Program Agency")]
    public string Agency { get; set; } = string.Empty;

    public string Jurisdiction { get; set; } = string.Empty;

    [Column("Jurisdiction Name")]
    public string? JurisdictionName { get; set; } = string.Empty;

    [Column("Incentives Area")]
    public string IncentivesArea { get; set; } = string.Empty;

    [Column("Incentives Type")]
    public string IncentivesType { get; set; } = string.Empty;

    [Ignore]
    public DateTime StartDateTimeUtc { get; set; }

    [Column("Program Start Date")]
    public string StartDateView { get; set; } = string.Empty;

    [Ignore]
    public DateTime EndDateTimeUtc { get; set; }

    [Column("Program End Date")]
    public string EndDateView { get; set; } = string.Empty;

    [Ignore]
    public DateTime CreatedDateTimeUtc { get; set; }

    [Column("Creation Date")]
    public string CreatedDateView { get; set; } = string.Empty;
}
