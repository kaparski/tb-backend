using TaxBeacon.UserManagement.Departments.Models;

namespace TaxBeacon.UserManagement.ServiceAreas.Models;

public class ServiceAreaDetailsDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public DepartmentDto? Department { get; set; }

    public DateTime CreatedDateTimeUtc { get; set; }
}
