using TaxBeacon.Administration.Departments.Models;

namespace TaxBeacon.Administration.Divisions.Models;

public class DivisionDetailsDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public DateTime CreatedDateTimeUtc { get; set; }

    public IList<DepartmentDto> Departments { get; set; } = null!;
}
