using TaxBeacon.Common.Enums.Accounts;

namespace TaxBeacon.Accounts.Common.Models;

public record CreateUpdatePhoneDto
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public string Number { get; init; } = null!;

    public PhoneType Type { get; init; }

    public string Extension { get; init; } = null!;
}
