namespace TaxBeacon.API.Controllers.Departments.Responses;

public record DepartmentUserResponse
{
    public Guid Id { get; init; }

    public string Email { get; init; } = null!;

    public string FullName { get; init; } = null!;

    public string? ServiceArea { get; init; }

    public string? Team { get; init; }

    public string? JobTitle { get; init; }
}
