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

    public ClientState? ClientState { get; init; }

    public Status? ClientStatus { get; init; }

    public ReferralState? ReferralState { get; init; }

    public Status? ReferralStatus { get; init; }
}
