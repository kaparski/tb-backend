namespace TaxBeacon.Accounts.Accounts.Models;

public record SalespersonDto
{
    public Guid Id { get; init; }
    public string FullName { get; init; } = null!;
}
