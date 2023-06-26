namespace TaxBeacon.Accounts.Accounts.Models;
public record ClientManagerDto
{
    public Guid ManagerId { get; init; }
    public AccountUserDto Manager { get; init; } = null!;
}
