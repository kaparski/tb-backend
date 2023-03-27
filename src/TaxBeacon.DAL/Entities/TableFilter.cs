using TaxBeacon.Common.Enums;

namespace TaxBeacon.DAL.Entities;

public class TableFilter
{
    public Guid TenantId { get; set; }

    public Guid UserId { get; set; }

    public Guid Id { get; set; }

    public EntityType TableType { get; set; }

    public string Name { get; set; } = null!;

    public string Configuration { get; set; } = null!;

    public TenantUser TenantUser { get; set; } = null!;
}
