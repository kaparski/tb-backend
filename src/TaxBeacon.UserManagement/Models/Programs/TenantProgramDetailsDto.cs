﻿using TaxBeacon.Common.Enums;

namespace TaxBeacon.UserManagement.Models.Programs;

public class TenantProgramDetailsDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Reference { get; set; } = string.Empty;

    public string Overview { get; set; } = string.Empty;

    public string LegalAuthority { get; set; } = string.Empty;

    public string Agency { get; set; } = string.Empty;

    public string Jurisdiction { get; set; } = string.Empty;

    public string State { get; set; } = string.Empty;

    public string County { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string IncentivesArea { get; set; } = string.Empty;

    public string IncentivesType { get; set; } = string.Empty;

    public DateTime StartDateTimeUtc { get; set; }

    public DateTime EndDateTimeUtc { get; set; }

    public DateTime CreatedDateTimeUtc { get; set; }

    public Status Status { get; set; }
}