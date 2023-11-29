using TaxBeacon.Common.Enums;

namespace TaxBeacon.API.Controllers.Contacts.Responses;

public record ContactAccountResponse
{
    public Guid Id { get; init; }

    public string Name { get; init; } = null!;

    public string Type { get; set; } = null!;

    public Status Status { get; set; }

    public string? ClientState { get; init; }

    public Status? ClientStatus { get; init; }

    public string? ReferralState { get; init; }

    public Status? ReferralStatus { get; init; }
}
