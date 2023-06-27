using TaxBeacon.Common.Enums;

namespace TaxBeacon.Administration.Tenants.Models;

public class TenantDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public DateTime CreatedDateTimeUtc { get; set; }

    public Status Status { get; set; }
}
