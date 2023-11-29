using TaxBeacon.Common.Enums.Accounts;
using TaxBeacon.DAL.Administration.Entities;

namespace TaxBeacon.DAL.Accounts.Common;

public abstract class PhoneBase: BaseEntity
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public PhoneType Type { get; set; }

    public string Number { get; set; } = null!;

    public string? Extension { get; set; }

    public Tenant Tenant { get; set; } = null!;
}
