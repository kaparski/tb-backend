using TaxBeacon.Common.Enums.Accounts;

namespace TaxBeacon.Accounts.Accounts.Models;

public sealed record CreateReferralDto
{
    public required OrganizationType OrganizationType { get; init; }

    public required ReferralType Type { get; init; }

    public IEnumerable<Guid> ReferralManagersIds { get; init; } = Enumerable.Empty<Guid>();
}
