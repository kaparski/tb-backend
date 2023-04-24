using TaxBeacon.Common.Enums;

namespace TaxBeacon.UserManagement.Models;

public class ServiceAreaDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;
}
