using TaxBeacon.API.Controllers.Departments.Responses;

namespace TaxBeacon.API.Controllers.ServiceAreas.Responses;

public class ServiceAreaDetailsResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public DepartmentResponse Department { get; set; } = null!;

    public DateTime CreatedDateTimeUtc { get; set; }
}
