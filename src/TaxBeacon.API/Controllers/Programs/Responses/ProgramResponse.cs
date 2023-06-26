using TaxBeacon.Common.Enums;

namespace TaxBeacon.API.Controllers.Programs.Responses;

public class ProgramResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Reference { get; set; }

    public string? Overview { get; set; }

    public string? LegalAuthority { get; set; }

    public string Agency { get; set; } = string.Empty;

    public Jurisdiction Jurisdiction { get; set; }

    public string? JurisdictionName { get; set; }

    public string? IncentivesArea { get; set; }

    public string? IncentivesType { get; set; }

    public DateTime? StartDateTimeUtc { get; set; }

    public DateTime? EndDateTimeUtc { get; set; }

    public DateTime CreatedDateTimeUtc { get; set; }
}
