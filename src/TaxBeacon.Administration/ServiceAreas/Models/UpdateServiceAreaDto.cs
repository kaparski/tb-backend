namespace TaxBeacon.Administration.ServiceAreas.Models;

public sealed record UpdateServiceAreaDto
{
    public string Name { get; init; } = string.Empty;

    public string? Description { get; init; }

    public Guid? DepartmentId { get; init; }
}
