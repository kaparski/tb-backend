using Npoi.Mapper.Attributes;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.Accounts.Accounts.Models;

public class ExportClientProspectDto
{
    [Column("Account Name")]
    public string Name { get; set; } = null!;

    [Column("Days Open")]
    public int DaysOpen { get; set; }

    public string? City { get; set; }

    public State? State { get; set; }

    public Status Status { get; set; }

}
