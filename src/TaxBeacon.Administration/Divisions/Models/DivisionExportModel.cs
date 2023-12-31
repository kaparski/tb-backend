﻿using Npoi.Mapper.Attributes;

namespace TaxBeacon.Administration.Divisions.Models;

public sealed class DivisionExportModel
{
    [Column("Division name")]
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Departments { get; set; } = string.Empty;

    [Column("Number of users")]
    public int NumberOfUsers { get; set; }

    [Ignore]
    public DateTime CreatedDateTimeUtc { get; set; }

    [Column("Creation date")]
    public string CreatedDateView { get; set; } = string.Empty;
}