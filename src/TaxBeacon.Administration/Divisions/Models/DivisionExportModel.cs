using Npoi.Mapper.Attributes;

namespace TaxBeacon.Administration.Divisions.Models;

public sealed class DivisionExportModel
{
    [Column("Division Name")]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string Departments { get; set; } = string.Empty;

    [Column("Number Of Users")]
    public int NumberOfUsers { get; set; }

    [Ignore]
    public DateTime CreatedDateTimeUtc { get; set; }

    [Column("Creation Date")]
    public string CreatedDateView { get; set; } = string.Empty;
}
