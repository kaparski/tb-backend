using Npoi.Mapper.Attributes;

namespace TaxBeacon.UserManagement.Models.Export;

public class ServiceAreaExportModel
{
    [Column("Service Area Name")]
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
