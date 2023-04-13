using Npoi.Mapper.Attributes;
using System.ComponentModel.DataAnnotations;

namespace TaxBeacon.UserManagement.Models.Export;

public class TeamExportModel
{
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    [Ignore]
    public DateTime CreatedDateTimeUtc { get; set; }

    [Column("Creation date")]
    public string CreatedDateView { get; set; } = string.Empty;

    [Display(Name = "Number of Users")]
    [Column("Number of Users")]
    public int NumberOfUsers { get; set; }
}
