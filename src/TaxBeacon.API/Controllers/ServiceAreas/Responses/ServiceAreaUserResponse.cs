namespace TaxBeacon.API.Controllers.ServiceAreas.Responses;

public class ServiceAreaUserResponse
{
    public Guid Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Team { get; set; } = string.Empty;

    public string JobTitle { get; set; } = string.Empty;
}
