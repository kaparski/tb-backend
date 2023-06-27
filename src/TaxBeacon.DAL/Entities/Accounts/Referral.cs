using TaxBeacon.Common.Enums;

namespace TaxBeacon.DAL.Entities.Accounts;

public class Referral: BaseEntity
{
    public Guid TenantId { get; set; }

    public Tenant Tenant { get; set; } = null!;

    public Guid AccountId { get; set; }

    public Account Account { get; set; } = null!;

    public string State { get; set; } = null!;

    public Status Status { get; set; }

    public Guid? ManagerId { get; set; }

    public TenantUser? Manager { get; set; }

    public string? NaicsCode { get; set; }
}
