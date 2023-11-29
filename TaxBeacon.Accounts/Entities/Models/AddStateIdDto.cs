using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Enums.Accounts;

namespace TaxBeacon.Accounts.Entities.Models;

public record AddStateIdDto
{
    public State State { get; init; }

    public StateIdType StateIdType { get; init; } = null!;

    public string StateIdCode { get; init; } = null!;

    public string? LocalJurisdiction { get; init; }
}
