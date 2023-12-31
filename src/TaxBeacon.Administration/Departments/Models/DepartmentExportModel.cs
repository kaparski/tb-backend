﻿using Npoi.Mapper.Attributes;

namespace TaxBeacon.Administration.Departments.Models;

public sealed class DepartmentExportModel
{
    [Column("Department Name")]
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    [Column("Service Area")]
    public string? ServiceAreas { get; set; }

    [Ignore]
    public DateTime CreatedDateTimeUtc { get; set; }

    [Column("Creation date")]
    public string CreatedDateView { get; set; } = string.Empty;

    [Column("Number of Users")]
    public int AssignedUsersCount { get; set; }
}

public sealed class DepartmentWithDivisionExportModel
{
    [Column("Department Name")]
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Division { get; set; } = string.Empty;

    [Column("Service Area")]
    public string? ServiceAreas { get; set; }

    [Ignore]
    public DateTime CreatedDateTimeUtc { get; set; }

    [Column("Creation date")]
    public string CreatedDateView { get; set; } = string.Empty;

    [Column("Number of Users")]
    public int AssignedUsersCount { get; set; }
}