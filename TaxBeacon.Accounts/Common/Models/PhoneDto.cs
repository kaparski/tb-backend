using TaxBeacon.Common.Enums.Accounts;

namespace TaxBeacon.Accounts.Common.Models;

public record PhoneDto
{
    public Guid Id { get; init; }

    public string Number { get; init; } = null!;

    public PhoneType Type { get; init; }

    public string Extension { get; init; } = null!;

    public DateTime CreatedDateTimeUtc { get; init; }
}
