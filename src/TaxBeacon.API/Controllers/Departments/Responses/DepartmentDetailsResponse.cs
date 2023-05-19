
using TaxBeacon.API.Controllers.JobTitles.Responses;
using TaxBeacon.API.Controllers.ServiceAreas.Responses;

namespace TaxBeacon.API.Controllers.Departments.Responses;

public class DepartmentDetailsResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public Guid? DivisionId { get; set; }

    public string Division { get; set; } = null!;

    public IList<ServiceAreaResponse> ServiceAreas { get; set; } = null!;

    public IList<JobTitleResponse> JobTitles { get; set; } = null!;

    public DateTime CreatedDateTimeUtc { get; set; }
}
