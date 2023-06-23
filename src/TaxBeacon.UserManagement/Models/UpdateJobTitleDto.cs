namespace TaxBeacon.UserManagement.Models;

public sealed record UpdateJobTitleDto
{
    public string Name { get; init; } = string.Empty;

    public string? Description { get; init; }

    public Guid? DepartmentId { get; init; }
}
