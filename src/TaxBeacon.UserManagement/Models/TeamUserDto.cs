namespace TaxBeacon.UserManagement.Models;

public class TeamUserDto
{
    public Guid Id { get; set; }

    public string Email { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string Department { get; set; } = string.Empty;

    public string ServiceArea { get; set; } = string.Empty;

    public string JobTitle { get; set; } = string.Empty;
}
