using TaxBeacon.API.Controllers.Departments.Responses;

namespace TaxBeacon.API.Controllers.JobTitles.Responses;

public class JobTitleDetailsResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DepartmentResponse Department { get; set; } = null!;

    public DateTime CreatedDateTimeUtc { get; set; }

    public DateTime? LastModifiedDateTimeUtc { get; init; }
}
