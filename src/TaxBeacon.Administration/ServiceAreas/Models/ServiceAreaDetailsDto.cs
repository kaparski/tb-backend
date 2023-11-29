using TaxBeacon.Administration.Departments.Models;

namespace TaxBeacon.Administration.ServiceAreas.Models;

public record ServiceAreaDetailsDto
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string? Description { get; init; }

    public DepartmentDto? Department { get; init; }

    public DateTime CreatedDateTimeUtc { get; init; }

    public DateTime? LastModifiedDateTimeUtc { get; init; }
}
