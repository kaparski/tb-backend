using Npoi.Mapper.Attributes;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.Accounts.Accounts.Models;

public class ClientExportDto
{
    [Column("Account Name")]
    public string Name { get; set; } = null!;

    public string? City { get; set; }

    public State? State { get; set; }

    [Column("Primary Contact")]
    public string PrimaryContactName { get; set; } = null!;

    public Status Status { get; set; }
}
