namespace TaxBeacon.Administration.Divisions.Models;

public record DivisionDto
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string? Description { get; init; }

    public DateTime CreatedDateTimeUtc { get; init; }

    public int NumberOfUsers { get; init; }

    public string Departments { get; init; } = string.Empty;

    public string Department { get; init; } = string.Empty;

    public IEnumerable<Guid>? DepartmentIds { get; set; }

    public int NumberOfDepartments { get; set; }
}
