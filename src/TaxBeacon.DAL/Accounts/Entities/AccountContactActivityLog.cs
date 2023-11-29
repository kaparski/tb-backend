using TaxBeacon.Common.Enums.Accounts.Activities;
using TaxBeacon.DAL.Administration.Entities;

namespace TaxBeacon.DAL.Accounts.Entities;

public class AccountContactActivityLog: BaseActivityLog<ContactEventType>
{
    public Guid TenantId { get; set; }

    public Guid AccountId { get; set; }

    public Guid ContactId { get; set; }

    public AccountContact AccountContact { get; set; } = null!;

    public Tenant Tenant { get; set; } = null!;
}
