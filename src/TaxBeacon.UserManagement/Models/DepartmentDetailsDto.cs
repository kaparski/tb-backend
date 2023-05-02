namespace TaxBeacon.UserManagement.Models;

public class DepartmentDetailsDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public DateTime CreatedDateTimeUtc { get; set; }

    public IList<ServiceAreaDto> ServiceAreas { get; set; } = null!;

    public Guid? DivisionId { get; set; }

    public string Division { get; set; } = null!;
}
