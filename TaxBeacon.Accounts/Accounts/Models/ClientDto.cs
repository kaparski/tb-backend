using TaxBeacon.Common.Enums;

namespace TaxBeacon.Accounts.Accounts.Models;

public record ClientDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string? City { get; set; }

    public State? State { get; set; }

    public string? PrimaryContactName { get; set; }

    public Status Status { get; set; }
}
