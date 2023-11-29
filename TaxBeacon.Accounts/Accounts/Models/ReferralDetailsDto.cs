using TaxBeacon.Common.Enums;

namespace TaxBeacon.Accounts.Accounts.Models;

public record ReferralDetailsDto
{
    public string State { get; init; } = null!;

    public Status? Status { get; init; }
};
