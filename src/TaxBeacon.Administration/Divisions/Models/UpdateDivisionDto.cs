namespace TaxBeacon.Administration.Divisions.Models;

public sealed record UpdateDivisionDto
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; init; }

    public IEnumerable<Guid>? DepartmentIds { get; init; }
}
