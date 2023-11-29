using TaxBeacon.Administration.Divisions.Models;
using TaxBeacon.Administration.JobTitles.Models;
using TaxBeacon.Administration.ServiceAreas.Models;

namespace TaxBeacon.Administration.Departments.Models;

public record DepartmentDetailsDto
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string? Description { get; init; }

    public DateTime CreatedDateTimeUtc { get; init; }

    public DateTime? LastModifiedDateTimeUtc { get; init; }

    public IList<ServiceAreaDto> ServiceAreas { get; init; } = null!;

    public IList<JobTitleDto> JobTitles { get; init; } = null!;

    public DivisionDto Division { get; init; } = null!;
}
