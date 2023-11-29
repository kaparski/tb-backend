using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Enums.Administration;

namespace TaxBeacon.API.Controllers.Programs.Responses;

public class TenantProgramDetailsResponse
{
    public Guid Id { get; set; }

    public string? Reference { get; set; }

    public string Name { get; set; } = string.Empty;

    public Jurisdiction Jurisdiction { get; set; }

    public string? Overview { get; set; }

    public string? LegalAuthority { get; set; }

    public string Agency { get; set; } = string.Empty;

    public string? State { get; set; }

    public string? County { get; set; }

    public string? City { get; set; }

    public string? IncentivesArea { get; set; }

    public string? IncentivesType { get; set; }

    public Status Status { get; set; }

    public DateTime? StartDateTimeUtc { get; set; }

    public DateTime? EndDateTimeUtc { get; set; }

    public DateTime? DeactivationDateTimeUtc { get; set; }

    public DateTime? ReactivationDateTimeUtc { get; set; }
}
