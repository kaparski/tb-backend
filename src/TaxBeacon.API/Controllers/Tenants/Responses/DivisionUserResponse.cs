namespace TaxBeacon.API.Controllers.Tenants.Responses;

public class DivisionUserResponse
{
    public Guid Id { get; set; }

    public string Email { get; set; } = string.Empty;

    public string FullName { get; set; }

    public string Department { get; set; }

    public string JobTitle { get; set; }
}
