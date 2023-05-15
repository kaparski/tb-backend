using TaxBeacon.API.Controllers.Departments.Responses;

namespace TaxBeacon.API.Controllers.Tenants.Responses;

public class DivisionDetailsResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public DateTime CreatedDateTimeUtc { get; set; }

    public IEnumerable<DivisionDepartmentResponse> Departments { get; set; } = Enumerable.Empty<DivisionDepartmentResponse>();
}
