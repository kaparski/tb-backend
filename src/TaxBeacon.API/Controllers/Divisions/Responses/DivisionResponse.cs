namespace TaxBeacon.API.Controllers.Divisions.Responses;

public class DivisionResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int NumberOfUsers { get; set; }

    public string? Description { get; set; }

    public DateTime CreatedDateTimeUtc { get; set; }

    public string Departments { get; set; } = string.Empty;

    public string Department { get; set; } = string.Empty;

    public IEnumerable<Guid>? DepartmentIds { get; set; }

    public int NumberOfDepartments { get; set; }
}
