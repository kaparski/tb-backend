namespace TaxBeacon.UserManagement.Departments.Models;

public class DepartmentDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public DateTime CreatedDateTimeUtc { get; set; }

    public int AssignedUsersCount { get; set; }

    public string? ServiceArea { get; set; }

    public IEnumerable<Guid>? ServiceAreaIds { get; set; }

    public Guid? DivisionId { get; set; }

    public string? Division { get; set; }

    public IEnumerable<Guid>? JobTitleIds { get; set; }
}
