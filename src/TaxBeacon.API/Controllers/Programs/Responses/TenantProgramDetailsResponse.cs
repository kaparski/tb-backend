using TaxBeacon.Common.Enums;

namespace TaxBeacon.API.Controllers.Programs.Responses;

public class TenantProgramDetailsResponse
{
    public Guid Id { get; set; }

    public string Reference { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public Jurisdiction Jurisdiction { get; set; }

    public string Overview { get; set; } = string.Empty;

    public string LegalAuthority { get; set; } = string.Empty;

    public string Agency { get; set; } = string.Empty;

    public string State { get; set; } = string.Empty;

    public string County { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string IncentivesArea { get; set; } = string.Empty;

    public string IncentivesType { get; set; } = string.Empty;

    public Status Status { get; set; }

    public DateTime StartDateTimeUtc { get; set; }

    public DateTime EndDateTimeUtc { get; set; }

    public DateTime CreatedDateTimeUtc { get; set; }
}
