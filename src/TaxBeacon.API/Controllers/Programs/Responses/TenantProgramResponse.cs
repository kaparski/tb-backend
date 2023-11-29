using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Enums.Administration;

namespace TaxBeacon.API.Controllers.Programs.Responses;

public record TenantProgramResponse
{
    public Guid Id { get; init; }

    public string? Reference { get; init; }

    public string Name { get; init; } = string.Empty;

    public Jurisdiction Jurisdiction { get; init; }

    public string? Overview { get; init; }

    public string? LegalAuthority { get; init; }

    public string Agency { get; init; } = string.Empty;

    public string? Department { get; init; }

    public string? ServiceArea { get; init; }

    public string? JurisdictionName { get; init; }

    public string? IncentivesArea { get; init; }

    public string? IncentivesType { get; init; }

    public Status Status { get; init; }

    public DateTime? StartDateTimeUtc { get; init; }

    public DateTime? EndDateTimeUtc { get; init; }

    public DateTime? DeactivationDateTimeUtc { get; init; }

    public DateTime? ReactivationDateTimeUtc { get; init; }
}

