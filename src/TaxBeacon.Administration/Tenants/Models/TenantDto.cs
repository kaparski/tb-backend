using TaxBeacon.Common.Enums;

namespace TaxBeacon.Administration.Tenants.Models;

public record TenantDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public DateTime CreatedDateTimeUtc { get; set; }

    public DateTime? LastModifiedDateTimeUtc { get; set; }

    public Status Status { get; set; }

    public bool? DivisionEnabled { get; set; }
}
