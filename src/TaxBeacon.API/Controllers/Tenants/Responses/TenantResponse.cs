using TaxBeacon.Common.Enums;

namespace TaxBeacon.API.Controllers.Tenants.Responses;

public record TenantResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public Status Status { get; set; }

    public DateTime CreatedDateTimeUtc { get; set; }

    public DateTime? LastModifiedDateTimeUtc { get; set; }

    public bool? DivisionEnabled { get; set; }
}
