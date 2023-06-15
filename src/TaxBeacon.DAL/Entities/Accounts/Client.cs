using TaxBeacon.Common.Accounts;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.DAL.Entities.Accounts;

public class Client: BaseEntity
{
    public Guid TenantId { get; set; }

    public Tenant Tenant { get; set; } = null!;

    public Guid AccountId { get; set; }

    public Account Account { get; set; } = null!;

    public ClientState State { get; set; }

    public Status Status { get; set; }

    public Guid? ManagerId { get; set; }

    public User? Manager { get; set; }

    public string? NaicsCode { get; set; }
}
