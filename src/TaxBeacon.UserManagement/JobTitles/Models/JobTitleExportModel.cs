using Npoi.Mapper.Attributes;

namespace TaxBeacon.UserManagement.JobTitles.Models;

public class JobTitleExportModel
{
    [Column("Job Title Name")]
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Department { get; set; } = string.Empty;

    [Ignore]
    public DateTime CreatedDateTimeUtc { get; set; }

    [Column("Creation Date")]
    public string CreatedDateView { get; set; } = string.Empty;

    [Column("Number of Users")]
    public int AssignedUsersCount { get; set; }
}
