using TaxBeacon.Common.Enums.Accounts;

namespace TaxBeacon.API.Shared.Requests;

public record CreateUpdatePhoneRequest
{
    public Guid? Id { get; init; }

    public string Number { get; init; } = null!;

    public PhoneType Type { get; init; }

    public string? Extension { get; init; } = null!;
}
