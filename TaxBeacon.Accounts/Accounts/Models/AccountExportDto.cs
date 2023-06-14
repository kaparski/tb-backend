using Npoi.Mapper.Attributes;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.Accounts.Accounts.Models;

public record AccountExportDto
{
    [Column("Account name")]
    public string Name { get; init; } = null!;
    
    public string? City { get; init; }
    
    public State State { get; init; }

    public string AccountType { get; init; } = null!;

    public string AccountManager { get; set; } = string.Empty;
}