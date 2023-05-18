﻿using TaxBeacon.Common.Enums;

namespace TaxBeacon.API.Controllers.Programs.Responses;

public class TenantProgramResponse
{
    public Guid Id { get; set; }

    public string Reference { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public Jurisdiction Jurisdiction { get; set; }

    public string Overview { get; set; } = string.Empty;

    public string LegalAuthority { get; set; } = string.Empty;

    public string Agency { get; set; } = string.Empty;

    public string Department { get; set; } = string.Empty;

    public string ServiceArea { get; set; } = string.Empty;

    public string JurisdictionName { get; set; } = string.Empty;

    public string IncentivesArea { get; set; } = string.Empty;

    public string IncentivesType { get; set; } = string.Empty;

    public Status Status { get; set; }

    public DateTime StartDateTimeUtc { get; set; }

    public DateTime EndDateTimeUtc { get; set; }

    public DateTime CreatedDateTimeUtc { get; set; }
}

