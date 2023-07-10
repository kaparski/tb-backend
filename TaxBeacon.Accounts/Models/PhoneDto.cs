namespace TaxBeacon.Accounts.Models;

public record PhoneDto
{
    public Guid Id { get; init; }
    
    public Guid AccountId { get; init; }

    public string Number { get; init; } = null!;

    public string Type { get; init; } = null!;

    public string Extension { get; init; } = null!;
}
