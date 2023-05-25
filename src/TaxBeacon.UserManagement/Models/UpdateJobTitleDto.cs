namespace TaxBeacon.UserManagement.Models;

public sealed class UpdateJobTitleDto
{
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public Guid DepartmentId { get; set; }
}
