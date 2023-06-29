using TaxBeacon.Common.Enums;

namespace TaxBeacon.DAL.Administration.Entities;

public class TableFilter
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid? TenantId { get; set; }

    public EntityType TableType { get; set; }

    public string Name { get; set; } = null!;

    public string Configuration { get; set; } = null!;

    public Tenant? Tenant { get; set; }

    public User User { get; set; } = null!;
}
