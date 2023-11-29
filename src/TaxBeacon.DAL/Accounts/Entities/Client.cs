using TaxBeacon.Common.Enums;

namespace TaxBeacon.DAL.Accounts.Entities;

public class Client: BaseDeletableEntity
{
    public Guid TenantId { get; set; }

    public Guid AccountId { get; set; }

    public string State { get; set; } = null!;

    public Status Status { get; set; }

    public Guid? PrimaryContactId { get; set; }

    public Contact? PrimaryContact { get; set; }

    public decimal? AnnualRevenue { get; set; }

    public int? EmployeeCount { get; set; }

    public int? FoundationYear { get; set; }

    public int DaysOpen { get; set; }

    public DateTime? ActivationDateTimeUtc { get; set; }

    public DateTime? DeactivationDateTimeUtc { get; set; }

    public DateTime? ReactivationDateTimeUtc { get; set; }

    public Tenant Tenant { get; set; } = null!;

    public Account Account { get; set; } = null!;

    public ICollection<ClientManager> ClientManagers { get; set; } = new HashSet<ClientManager>();
}
