﻿
namespace TaxBeacon.Administration.JobTitles.Models;

public class JobTitleDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public Guid? DepartmentId { get; set; }

    public string? Department { get; set; }

    public DateTime CreatedDateTimeUtc { get; set; }

    public int AssignedUsersCount { get; set; }
}
