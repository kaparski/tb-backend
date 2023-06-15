using TaxBeacon.Common.Accounts;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.Accounts.Accounts.Models;

public record AccountDto
{
    public Guid Id { get; init; }

    public string Name { get; init; } = null!;
    
    public string? City { get; init; }
    
    public State State { get; init; }

    public string AccountType { get; init; } = null!;
    
    public ClientState? ClientState { get; set; }

    public Status? ClientStatus { get; set; }

    public ReferralState? ReferralState { get; set; }

    public Status? ReferralStatus { get; set; }
}
