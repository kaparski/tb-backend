using TaxBeacon.Common.Enums;

namespace TaxBeacon.Accounts.Accounts.Models;

public class ClientProspectDto
{
    public Guid AccountId { get; set; }

    public string Name { get; set; } = null!;

    public string? City { get; set; }

    public int DaysOpen { get; set; }

    public State? State { get; set; }

    public Status Status { get; set; }
}
