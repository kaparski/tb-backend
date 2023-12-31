﻿using TaxBeacon.Administration.Divisions.Models;
using TaxBeacon.Administration.JobTitles.Models;
using TaxBeacon.Administration.ServiceAreas.Models;

namespace TaxBeacon.Administration.Departments.Models;

public class DepartmentDetailsDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public DateTime CreatedDateTimeUtc { get; set; }

    public IList<ServiceAreaDto> ServiceAreas { get; set; } = null!;

    public IList<JobTitleDto> JobTitles { get; set; } = null!;

    public DivisionDto Division { get; set; } = null!;
}
