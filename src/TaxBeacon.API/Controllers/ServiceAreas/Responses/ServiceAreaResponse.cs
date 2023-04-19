
namespace TaxBeacon.API.Controllers.ServiceAreas.Responses;

public class ServiceAreaResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;
}
