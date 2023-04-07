using TaxBeacon.Common.Enums;

namespace TaxBeacon.API.Controllers.Tenants.Responses;

public class TenantResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public Status Status { get; set; }

    public DateTime CreatedDateTimeUtc { get; set; }
}
