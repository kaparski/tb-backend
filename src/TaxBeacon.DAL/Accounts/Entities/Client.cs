using TaxBeacon.Common.Enums;
using TaxBeacon.DAL.Administration.Entities;

namespace TaxBeacon.DAL.Accounts.Entities;

public class Client: BaseEntity
{
    public Guid TenantId { get; set; }

    public Tenant Tenant { get; set; } = null!;

    public Guid AccountId { get; set; }

    public Account Account { get; set; } = null!;

    public string State { get; set; } = null!;

    public Status Status { get; set; }

    public ICollection<ClientManager> ClientManagers { get; set; } = new HashSet<ClientManager>();

    public Guid? PrimaryContactId { get; set; }

    public Contact? PrimaryContact { get; set; }

    public string? NaicsCode { get; set; }

    public decimal? AnnualRevenue { get; set; }

    public int? EmployeeCount { get; set; }

    public int? FoundationYear { get; set; }

    public DateTime? DeactivationDateTimeUtc { get; set; }

    public DateTime? ReactivationDateTimeUtc { get; set; }
}
