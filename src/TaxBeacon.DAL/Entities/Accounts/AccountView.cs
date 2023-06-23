using TaxBeacon.Common.Enums;

namespace TaxBeacon.DAL.Entities.Accounts;

public class AccountView: BaseEntity
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string Name { get; set; } = null!;

    public State? State { get; set; }

    public string? City { get; set; }

    public string Website { get; set; } = null!;

    public string AccountType { get; set; } = null!;

    public string? ClientState { get; set; }

    public Status? ClientStatus { get; set; }

    public string? ReferralState { get; set; }

    public Status? ReferralStatus { get; set; }
}
