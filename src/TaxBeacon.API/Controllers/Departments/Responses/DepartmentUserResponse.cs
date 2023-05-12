namespace TaxBeacon.API.Controllers.Tenants.Responses;

public class DepartmentUserResponse
{
    public Guid Id { get; set; }

    public string Email { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string ServiceArea { get; set; } = string.Empty;

    public string Team { get; set; } = string.Empty;

    public string JobTitle { get; set; } = string.Empty;
}
