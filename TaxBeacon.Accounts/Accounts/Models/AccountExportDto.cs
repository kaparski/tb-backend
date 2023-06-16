using Npoi.Mapper.Attributes;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.Accounts.Accounts.Models;

public record AccountExportDto
{
    [Column("Account Name")]
    public string Name { get; init; } = null!;
    
    [Column("Account Type")]
    public string AccountType { get; init; } = null!;
    
    [Column("City")]
    public string? City { get; init; }
    
    [Column("State")]
    public State State { get; init; }

    [Column("Account Managers")]
    public string AccountManagers { get; set; } = string.Empty;
}