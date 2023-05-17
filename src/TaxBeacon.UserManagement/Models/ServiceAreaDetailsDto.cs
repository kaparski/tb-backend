namespace TaxBeacon.UserManagement.Models;

public class ServiceAreaDetailsDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Department { get; set; } = string.Empty;

    public DateTime CreatedDateTimeUtc { get; set; }
}