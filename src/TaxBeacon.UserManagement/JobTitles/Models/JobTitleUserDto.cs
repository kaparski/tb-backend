namespace TaxBeacon.UserManagement.JobTitles.Models;

public class JobTitleUserDto
{
    public Guid Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Department { get; set; } = string.Empty;

    public string ServiceArea { get; set; } = string.Empty;

    public string Team { get; set; } = string.Empty;
}
