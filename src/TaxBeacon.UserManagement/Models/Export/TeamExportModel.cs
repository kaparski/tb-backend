using Npoi.Mapper.Attributes;

namespace TaxBeacon.UserManagement.Models.Export;

public class TeamExportModel
{
    [Column("Team name")]
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    [Ignore]
    public DateTime CreatedDateTimeUtc { get; set; }

    [Column("Creation Date")]
    public string CreatedDateView { get; set; } = string.Empty;

    [Column("Number of Users")]
    public int NumberOfUsers { get; set; }
}
