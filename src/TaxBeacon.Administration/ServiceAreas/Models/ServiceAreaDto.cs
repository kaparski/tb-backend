namespace TaxBeacon.Administration.ServiceAreas.Models;

public record ServiceAreaDto
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string? Description { get; init; }

    public Guid? DepartmentId { get; init; }

    public string? Department { get; init; }

    public DateTime CreatedDateTimeUtc { get; init; }

    public int AssignedUsersCount { get; init; }
}
