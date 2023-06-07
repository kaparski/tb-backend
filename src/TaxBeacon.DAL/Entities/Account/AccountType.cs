using TaxBeacon.Common.Account;

namespace TaxBeacon.DAL.Entities.Account;

public class AccountType: BaseEntity
{
    public Guid TenantId { get; set; }

    public Tenant Tenant { get; set; } = null!;

    public Guid AccountId { get; set; }

    public Account Account { get; set; } = null!;

    public TaxBeacon.Common.Account.AccountType Type { get; set; }

    public AccountState State { get; set; }

    public bool? IsEnabled { get; set; }

    public DateTime? ToggleDate { get; set; }
}
