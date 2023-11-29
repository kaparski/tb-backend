using TaxBeacon.API.Controllers.Divisions.Responses;
using TaxBeacon.API.Controllers.JobTitles.Responses;
using TaxBeacon.API.Controllers.ServiceAreas.Responses;

namespace TaxBeacon.API.Controllers.Departments.Responses;

public record DepartmentDetailsResponse
{
    public Guid Id { get; init; }

    public string Name { get; init; } = null!;

    public string? Description { get; init; }

    public DivisionResponse? Division { get; init; }

    public IList<ServiceAreaResponse> ServiceAreas { get; init; } = null!;

    public IList<JobTitleResponse> JobTitles { get; init; } = null!;

    public DateTime CreatedDateTimeUtc { get; init; }

    public DateTime? LastModifiedDateTimeUtc { get; init; }
}
