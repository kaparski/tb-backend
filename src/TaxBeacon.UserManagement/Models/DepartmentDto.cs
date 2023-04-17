namespace TaxBeacon.UserManagement.Models;

public class DepartmentDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public DateTime CreatedDateTimeUtc { get; set; }

    public int AssignedUsersCount { get; set; }
}
