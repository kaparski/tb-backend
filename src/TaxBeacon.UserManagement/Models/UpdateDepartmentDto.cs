namespace TaxBeacon.UserManagement.Models;

public sealed class UpdateDepartmentDto
{
    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public Guid DivisionId { get; set; }

    public IEnumerable<Guid> ServiceAreasIds { get; set; } = null!;

    public IEnumerable<Guid> JobTitlesIds { get; set; } = null!;
}
