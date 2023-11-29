
namespace TaxBeacon.API.Controllers.Departments.Responses;

public record DepartmentResponse
{
    public Guid Id { get; init; }

    public string Name { get; init; } = null!;

    public string? Description { get; init; }

    public Guid? DivisionId { get; init; }

    public string? Division { get; init; }

    public string? ServiceArea { get; init; }

    public IEnumerable<Guid>? ServiceAreaIds { get; init; }

    public IEnumerable<Guid>? JobTitleIds { get; init; }

    public DateTime CreatedDateTimeUtc { get; init; }

    public int AssignedUsersCount { get; init; }
}
