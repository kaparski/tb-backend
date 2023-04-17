
namespace TaxBeacon.API.Controllers.Tenants.Responses;

public class DepartmentResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public DateTime CreatedDateTimeUtc { get; set; }

    public int AssignedUsersCount { get; set; }
}
