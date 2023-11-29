using TaxBeacon.Common.Enums;
using TaxBeacon.DAL.Administration.Entities;

namespace TaxBeacon.DAL.Accounts.Entities;

public class AccountContact
{
    public Guid TenantId { get; set; }

    public Guid AccountId { get; set; }

    public Guid ContactId { get; set; }

    public string Type { get; set; } = null!;

    // TODO: Create a base class or interface that contains status-related fields, and move the common logic for changing these fields from services to a single location
    public Status Status { get; set; }

    public DateTime? DeactivationDateTimeUtc { get; set; }

    public DateTime? ReactivationDateTimeUtc { get; set; }

    public Account Account { get; set; } = null!;

    public Contact Contact { get; set; } = null!;

    public Tenant Tenant { get; set; } = null!;

    public ICollection<AccountContactActivityLog> AccountContactActivityLogs { get; set; } =
        new HashSet<AccountContactActivityLog>();
}
