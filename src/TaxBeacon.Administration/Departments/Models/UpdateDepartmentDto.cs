namespace TaxBeacon.Administration.Departments.Models;

public sealed record UpdateDepartmentDto
{
    public string Name { get; init; } = null!;
    public string? Description { get; init; }

    public Guid? DivisionId { get; init; }

    public IEnumerable<Guid> ServiceAreasIds { get; init; } = null!;

    public IEnumerable<Guid> JobTitlesIds { get; init; } = null!;
}

