using TaxBeacon.Administration.Departments.Models;

namespace TaxBeacon.Administration.Divisions.Models;

public record DivisionDetailsDto
{
    public Guid Id { get; init; }

    public string Name { get; init; } = null!;

    public string? Description { get; init; }

    public DateTime CreatedDateTimeUtc { get; init; }

    public DateTime? LastModifiedDateTimeUtc { get; init; }

    public IList<DepartmentDto> Departments { get; init; } = null!;
}
